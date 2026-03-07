namespace HintServiceMeow.Core.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Extension;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models;
    using HintServiceMeow.Core.Models.Arguments;
    using HintServiceMeow.Core.Models.Hints;
    using HintServiceMeow.Core.Utilities.Parser;
    using HintServiceMeow.Core.Utilities.Tools;
    using HintServiceMeow.Core.Utilities.UnityAdaptors;

    /// <summary>
    /// Represents a player's display, responsible for managing hints and rendering them on the player's screen.
    /// </summary>
    public class PlayerDisplay : IPlayerDisplay, IDestructible
    {
        private static readonly HashSet<PlayerDisplay> PlayerDisplayList = [];
        private static readonly object PlayerDisplayListLock = new();

        private readonly List<IDisplayOutput> displayOutputs = [];

        private readonly IPlayerContext playerContext; // Initialize in constructor
        private readonly HintCollection hintCollection = new();
        private readonly ITaskScheduler updateScheduler; // Initialize in constructor

        private readonly object displayOutputsLock = new();
        private readonly object currentParserTaskLock = new();

        private IHintParser hintParser = new HintParser();
        private ICompatibilityAdaptor adapter; // Initialize in constructor

        private IMainThreadDispatcher mainThreadDispatcher = new UnityMainThreadDispatcher();

        private ICoroutine coroutine; // Initialize in constructor
        private ICoroutineRunner coroutineRunner = new UnityCoroutineRunner();

        private Task? currentParserTask;

        private volatile bool isDestructed = false;

        internal PlayerDisplay(
            IPlayerContext playerContext,
            HintCollection? displayHints = null,
            ITaskScheduler? updateScheduler = null,
            ICompatibilityAdaptor? adaptor = null,
            IHintParser? hintParser = null,
            IEnumerable<IDisplayOutput>? displayOutputs = null,
            IMainThreadDispatcher? dispatcher = null,
            ICoroutineRunner? coroutineRunner = null)
        {
            // Initialize each components
            this.playerContext = playerContext ?? throw new ArgumentNullException(nameof(playerContext));

            if (displayHints != null)
                this.hintCollection = displayHints;
            if (hintParser != null)
                this.hintParser = hintParser;
            if (displayOutputs != null)
                this.displayOutputs = displayOutputs.ToList();
            if (dispatcher != null)
                this.mainThreadDispatcher = dispatcher;
            if (coroutineRunner != null)
                this.coroutineRunner = coroutineRunner;

            adapter = adaptor ?? new CompatibilityAdaptor(this); // Default compatibility adaptor
            this.updateScheduler = updateScheduler ?? new TaskScheduler(); // Default task scheduler with zero interval

            // When collection changed, update the content on player's screen
            this.hintCollection.CollectionChanged += OnCollectionChanged;

            // Initialize update scheduler. Make update scheduler wait for a cycle when the previous parper is still running. Set action of the scheduler to start parser task.
            this.updateScheduler.InvokeUntilSuccess = true;
            this.updateScheduler.Start(TimeSpan.Zero, () =>
            {
                lock (currentParserTaskLock)
                {
                    if (currentParserTask != null)
                        return false; // If a parser task is already running, wait till next cycle to update
                }

                this.updateScheduler.Pause(); // Pause action until the parser task is finishing
                StartParserTask();

                return true; // Success
            });

            // Start the main coroutine on main thread
            coroutine = this.coroutineRunner.StartCoroutine(CoroutineMethod());
        }

        private PlayerDisplay(ReferenceHub referenceHub)
            : this(new ReferenceHubContext(referenceHub))
        {
            if (referenceHub is null)
                throw new ArgumentNullException(nameof(referenceHub));

            // Do not add display output for host
            if (referenceHub.IsHost)
                return;

            displayOutputs.Add(new ScpslDisplayOutput(referenceHub.connectionToClient));
        }

        /// <summary>
        /// Represents a method that handles the <see cref="UpdateAvailable"/> event.
        /// </summary>
        /// <param name="ev">The event arguments containing the player display context.</param>
        public delegate void UpdateAvailableEventHandler(UpdateAvailableEventArg ev);

        /// <summary>
        /// Invoked every tick when the player's display is ready to receive a hint update.
        /// </summary>
        public event UpdateAvailableEventHandler? UpdateAvailable;

        /// <summary>
        /// Gets the player this instance binds to.
        /// </summary>
        public ReferenceHub? ReferenceHub => playerContext is ReferenceHubContext context ? context.ReferenceHub : throw new NullReferenceException();

        /// <summary>
        /// Gets or sets the parser used to convert hint collections into display messages.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the assigned value is <see langword="null"/>.</exception>
        public IHintParser HintParser
        {
            get => hintParser;
            set => hintParser = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the compatibility adaptor used to send hints to the player.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when the assigned value is <see langword="null"/>.</exception>
        public ICompatibilityAdaptor CompatibilityAdaptor
        {
            get => adapter;
            set => adapter = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or creates the <see cref="PlayerDisplay"/> instance for the specified reference hub.
        /// </summary>
        /// <param name="referenceHub">The <see cref="global::ReferenceHub"/> that owns the <see cref="PlayerDisplay"/>.</param>
        /// <returns>The <see cref="PlayerDisplay"/> assigned to the given <see cref="global::ReferenceHub"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="referenceHub"/> is <see langword="null"/>.</exception>
        public static PlayerDisplay Get(ReferenceHub referenceHub)
        {
            if (referenceHub is null)
                throw new ArgumentNullException(nameof(referenceHub));

            lock (PlayerDisplayListLock)
            {
                foreach (PlayerDisplay playerDisplay in PlayerDisplayList)
                {
                    if (playerDisplay.playerContext is ReferenceHubContext referenceHubContext
                        && referenceHubContext.ReferenceHub == referenceHub)
                    {
                        return playerDisplay;
                    }
                }

                // Create new one if not found.
                PlayerDisplay newPlayerDisplay = new(referenceHub);
                PlayerDisplayList.Add(newPlayerDisplay);
                return newPlayerDisplay;
            }
        }

        /// <summary>
        /// Gets or creates the <see cref="PlayerDisplay"/> instance for the specified LabApi player.
        /// </summary>
        /// <param name="player">The LabApi player whose display is retrieved or created.</param>
        /// <returns>The <see cref="PlayerDisplay"/> assigned to the given player.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="player"/> is <see langword="null"/>.</exception>
        public static PlayerDisplay Get(LabApi.Features.Wrappers.Player player)
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));

            return Get(player.ReferenceHub);
        }

#if EXILED
        /// <summary>
        /// Get the PlayerDisplay instance of the player. If the instance have not been created yet, then it will create one.
        /// Not Thread Safe.
        /// </summary>
        /// <param name="player">The owner of the <see cref="PlayerDisplay"/>.</param>
        /// <returns>The players <see cref="PlayerDisplay"/>.</returns>
        public static PlayerDisplay Get(Exiled.API.Features.Player player)
        {
            if (player is null)
                throw new ArgumentNullException(nameof(player));

            return Get(player.ReferenceHub);
        }
#endif

        /// <summary>
        /// Forces an immediate display update. Only needed when using <see cref="HintSyncSpeed.UnSync"/>.
        /// </summary>
        /// <param name="useFastUpdate">If <see langword="true"/>, schedules the update as soon as possible; otherwise applies a short delay.</param>
        public void ForceUpdate(bool useFastUpdate = false)
        {
            ScheduleUpdate(useFastUpdate ? 0f : 0.3f);
        }

        /// <summary>
        /// Sets the minimum interval between each updates.
        /// </summary>
        /// <remarks>Use this method to control how frequently updates are allowed to occur. Setting a
        /// longer interval can help reduce resource usage by limiting update frequency.</remarks>
        /// <param name="interval">The minimum time interval that must elapse between updates. Must be a positive value.</param>
        public void SetMinUpdateInterval(TimeSpan interval)
        {
            updateScheduler.MinInterval = interval;
        }

        /// <summary>
        /// Registers an additional display output target that will receive rendered hint content.
        /// </summary>
        /// <param name="output">The display output to add.</param>
        public void AddDisplayOutput(IDisplayOutput output)
        {
            lock (displayOutputsLock)
            {
                displayOutputs.Add(output);
            }
        }

        /// <summary>
        /// Removes the specified display output target.
        /// </summary>
        /// <param name="output">The display output to remove.</param>
        public void RemoveDisplayOutput(IDisplayOutput output)
        {
            lock (displayOutputsLock)
            {
                displayOutputs.Remove(output);
            }
        }

        /// <summary>
        /// Removes all display output targets of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of display output to remove.</typeparam>
        public void RemoveDisplayOutput<T>()
            where T : IDisplayOutput
        {
            lock (displayOutputsLock)
            {
                displayOutputs.RemoveAll(x => x is T);
            }
        }

        /// <summary>
        /// Adds a hint to this player's display under the calling assembly's group.
        /// </summary>
        /// <param name="hint">The hint to add. Ignored if <see langword="null"/>.</param>
        public void AddHint(AbstractHint? hint)
        {
            if (hint is null)
                return;

            InternalAddHint(Assembly.GetCallingAssembly().FullName, hint);
        }

        /// <summary>
        /// Adds a collection of hints to this player's display under the calling assembly's group.
        /// </summary>
        /// <param name="hints">The hints to add. Ignored if <see langword="null"/>.</param>
        public void AddHint(IEnumerable<AbstractHint>? hints)
        {
            if (hints is null)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;

            foreach (AbstractHint hint in hints)
            {
                InternalAddHint(groupName, hint);
            }
        }

        /// <summary>
        /// Adds one or more hints to this player's display under the calling assembly's group.
        /// </summary>
        /// <param name="hints">The hints to add. Ignored if <see langword="null"/> or empty.</param>
        public void AddHint(params AbstractHint[]? hints)
        {
            if (hints is null || hints.Length == 0)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;
            foreach (AbstractHint hint in hints)
            {
                InternalAddHint(groupName, hint);
            }
        }

        /// <summary>
        /// Only use this if you know what you are doing. Add a hint to a specified group.
        /// </summary>
        /// <param name="hint">Hint added.</param>
        /// <param name="groupName">Group the hint will be assigned to.</param>
        public void AddHint(AbstractHint? hint, string groupName)
        {
            if (hint is null)
                return;

            InternalAddHint(groupName, hint);
        }

        /// <summary>
        /// Adds a hint to the player's display and schedules its removal or hiding after the specified duration.
        /// </summary>
        /// <param name="hint">The hint to show.</param>
        /// <param name="duration">The duration in seconds before the after-show action is applied.</param>
        /// <param name="afterShow">The action to take after the duration elapses.</param>
        public void ShowHint(AbstractHint hint, float duration = 7f, AfterShowAction afterShow = AfterShowAction.Remove)
        {
            if (hint is null)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;

            this.InternalAddHint(groupName, hint);

            switch (afterShow)
            {
                case AfterShowAction.Remove:
                    this.RemoveAfter(hint, duration);
                    break;
                case AfterShowAction.Hide:
                    hint.HideAfter(duration);
                    break;
            }
        }

        /// <summary>
        /// Adds a collection of hints to the player's display and schedules their removal or hiding after the specified duration.
        /// </summary>
        /// <param name="hints">The hints to show.</param>
        /// <param name="duration">The duration in seconds before the after-show action is applied.</param>
        /// <param name="afterShow">The action to take after the duration elapses.</param>
        public void ShowHint(IEnumerable<AbstractHint> hints, float duration = 7f, AfterShowAction afterShow = AfterShowAction.Remove)
        {
            if (hints is null)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;

            foreach (AbstractHint hint in hints)
            {
                this.InternalAddHint(groupName, hint);

                switch (afterShow)
                {
                    case AfterShowAction.Remove:
                        this.RemoveAfter(hint, duration);
                        break;
                    case AfterShowAction.Hide:
                        hint.HideAfter(duration);
                        break;
                }
            }
        }

        /// <summary>
        /// Removes the specified hint from the calling assembly's group.
        /// </summary>
        /// <param name="hint">The hint to remove. Ignored if <see langword="null"/>.</param>
        public void RemoveHint(AbstractHint? hint)
        {
            if (hint is null)
                return;

            InternalRemoveHint(Assembly.GetCallingAssembly().FullName, hint);
        }

        /// <summary>
        /// Removes a collection of hints from the calling assembly's group.
        /// </summary>
        /// <param name="hints">The hints to remove. Ignored if <see langword="null"/>.</param>
        public void RemoveHint(IEnumerable<AbstractHint>? hints)
        {
            if (hints is null)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;
            foreach (AbstractHint hint in hints)
            {
                InternalRemoveHint(groupName, hint);
            }
        }

        /// <summary>
        /// Removes one or more hints from the calling assembly's group.
        /// </summary>
        /// <param name="hints">The hints to remove. Ignored if <see langword="null"/> or empty.</param>
        public void RemoveHint(params AbstractHint[]? hints)
        {
            if (hints is null || hints.Length == 0)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;
            foreach (AbstractHint hint in hints)
            {
                InternalRemoveHint(groupName, hint);
            }
        }

        /// <summary>
        /// Only use this if you know what you are doing. Removes the specified hint from the given group.
        /// </summary>
        /// <param name="hint">The hint to remove. Ignored if <see langword="null"/>.</param>
        /// <param name="groupName">The group name to remove the hint from.</param>
        public void RemoveHint(AbstractHint? hint, string groupName)
        {
            if (hint is null)
                return;

            InternalRemoveHint(groupName, hint);
        }

        /// <summary>
        /// Removes all hints with the specified identifier from the calling assembly's group.
        /// </summary>
        /// <param name="id">The identifier of the hints to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public void RemoveHint(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to RemoveHint");

            InternalRemoveHint(Assembly.GetCallingAssembly().FullName, id);
        }

        /// <summary>
        /// Removes the hint with the specified <see cref="Guid"/> from the calling assembly's group.
        /// </summary>
        /// <param name="id">The unique identifier of the hint to remove.</param>
        public void RemoveHint(Guid id)
        {
            InternalRemoveHint(Assembly.GetCallingAssembly().FullName, id);
        }

        /// <summary>
        /// Removes all hints registered by the calling assembly.
        /// </summary>
        public void ClearHint()
        {
            InternalClearHint(Assembly.GetCallingAssembly().FullName);
        }

        /// <summary>
        /// Returns the first hint registered by the calling assembly that matches the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to search for.</param>
        /// <returns>The matching <see cref="AbstractHint"/>, or <see langword="null"/> if none is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public AbstractHint? GetHint(string? id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to GetHint");

            return InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Returns the first hint registered by the calling assembly that matches the specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">The unique identifier to search for.</param>
        /// <returns>The matching <see cref="AbstractHint"/>, or <see langword="null"/> if none is found.</returns>
        public AbstractHint? GetHint(Guid guid)
            => InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Guid == guid).FirstOrDefault();

        /// <summary>
        /// Returns all hints registered by the calling assembly that match the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to filter hints by.</param>
        /// <returns>An enumerable sequence of matching hints.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public IEnumerable<AbstractHint> GetHints(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to GetHints");

            return InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id);
        }

        /// <summary>
        /// Returns all hints registered by the calling assembly.
        /// </summary>
        /// <returns>An enumerable sequence of all hints belonging to the calling assembly.</returns>
        public IEnumerable<AbstractHint> GetHints()
        {
            return InternalGetHints(Assembly.GetCallingAssembly().FullName);
        }

        /// <summary>
        /// Determines whether any hint registered by the calling assembly has the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to search for.</param>
        /// <returns><see langword="true"/> if a matching hint exists; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public bool HasHint(string id)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to HasHint");

            return InternalGetHints(Assembly.GetCallingAssembly().FullName, hint => hint.Id == id).Any();
        }

        /// <summary>
        /// Determines whether any hint registered by the calling assembly matches the specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">The unique identifier to search for.</param>
        /// <returns><see langword="true"/> if a matching hint exists; otherwise <see langword="false"/>.</returns>
        public bool HasHint(Guid guid)
        {
            return InternalGetHints(Assembly.GetCallingAssembly().FullName, hint => hint.Guid == guid).Any();
        }

        /// <summary>
        /// Attempts to retrieve the first hint registered by the calling assembly that matches the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to search for.</param>
        /// <param name="hint">When this method returns, contains the matching hint, or <see langword="null"/> if none was found.</param>
        /// <returns><see langword="true"/> if a matching hint was found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
#nullable disable
        public bool TryGetHint(string id, out AbstractHint hint)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to TryGetHint");

            hint = InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id).FirstOrDefault();

            return hint != null;
        }

        /// <summary>
        /// Attempts to retrieve the first hint registered by the calling assembly that matches the specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">The unique identifier to search for.</param>
        /// <param name="hint">When this method returns, contains the matching hint, or <see langword="null"/> if none was found.</param>
        /// <returns><see langword="true"/> if a matching hint was found; otherwise <see langword="false"/>.</returns>
        public bool TryGetHint(Guid guid, out AbstractHint hint)
        {
            hint = InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Guid == guid).FirstOrDefault();
            return hint != null;
        }
#nullable restore

        /// <summary>
        /// Attempts to retrieve all hints registered by the calling assembly that match the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to search for.</param>
        /// <param name="hints">When this method returns, contains the matching hints.</param>
        /// <returns><see langword="true"/> if at least one matching hint was found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public bool TryGetHints(string? id, out IEnumerable<AbstractHint> hints)
        {
            if (id is null)
                throw new ArgumentNullException(nameof(id));

            if (id == string.Empty)
                throw new ArgumentException("A empty string had been passed to TryGetHints");

            hints = InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id);
            return hints.Any();
        }

        void IDestructible.Destruct()
        {
            isDestructed = true; // Mark as destroyed to prevent further actions

            coroutine.Kill(); // Stop coroutine

            // Clear collection's reference to this pd
            hintCollection.CollectionChanged -= OnCollectionChanged;

            // Clear hint's reference to this pd
            foreach (AbstractHint hint in hintCollection.GetHints(null))
            {
                hint.PropertyChanged -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;
            }

            UpdateAvailable = null; // Clear event

            // Clear pd's reference to hints
            hintCollection.ClearHints(null);

            ((IDestructible)updateScheduler).Destruct(); // Stop task scheduler's coroutine

            ((IDestructible)adapter).Destruct(); // Stop compatibility adaptor's coroutine
        }

        /// <summary>
        /// Not thread safe.
        /// </summary>
        /// <param name="referenceHub">The owner of the PlayerDisplay to destroy.</param>
        internal static void Destruct(ReferenceHub referenceHub)
        {
            if (referenceHub is null)
                throw new ArgumentNullException(nameof(referenceHub));

            ReferenceHubContext context = new(referenceHub);

            lock (PlayerDisplayListLock)
            {
                PlayerDisplay? pd = PlayerDisplayList.FirstOrDefault(x => x.playerContext.Equals(context));

                if (pd is null)
                    return;

                ((IDestructible)pd).Destruct();

                PlayerDisplayList.Remove(pd); // Remove from the reference list
            }
        }

        internal void InternalAddHint(string name, AbstractHint hint)
        {
            hint.PropertyChanged += OnHintUpdate;
            UpdateAvailable += hint.TryUpdateHint;

            hintCollection.AddHint(name, hint);
        }

        internal void InternalRemoveHint(string? name, AbstractHint hint)
        {
            hint.PropertyChanged -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            hintCollection.RemoveHint(name, hint);
        }

        internal void InternalRemoveHint(string name, Guid guid)
        {
            AbstractHint? hint = hintCollection.GetHints(name).FirstOrDefault(x => x.Guid.Equals(guid));

            if (hint == null)
                return;

            hint.PropertyChanged -= OnHintUpdate;
            UpdateAvailable -= hint.TryUpdateHint;

            hintCollection.RemoveHint(name, x => x.Guid.Equals(guid));
        }

        internal void InternalRemoveHint(string name, string id)
        {
            IEnumerable<AbstractHint> removeList = hintCollection.GetHints(name).Where(predicate => predicate.Id == id);

            foreach (AbstractHint hint in removeList)
            {
                hint.PropertyChanged -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;
            }

            hintCollection.RemoveHint(name, x => x.Id.Equals(id));
        }

        internal void InternalClearHint(string name)
        {
            foreach (AbstractHint hint in hintCollection.GetHints(name).ToList())
            {
                hint.PropertyChanged -= OnHintUpdate;
                UpdateAvailable -= hint.TryUpdateHint;
            }

            hintCollection.ClearHints(name);
        }

        internal IReadOnlyList<AbstractHint> InternalGetHints(string name)
        {
            return hintCollection.GetHints(name);
        }

        internal IReadOnlyList<AbstractHint> InternalGetHints(string name, Func<AbstractHint, bool> predicate)
        {
            return hintCollection.GetHints(name, predicate);
        }

        internal void ShowCompatibilityHint(string assemblyName, string? content, float duration) => adapter.ShowHint(new CompatibilityAdaptorArg(assemblyName, content, duration));

        private IEnumerator<float> CoroutineMethod()
        {
            while (true)
            {
                yield return -1f;

                // If player has quit, then stop the coroutine
                if (!playerContext.IsValid())
                    break;

                // Reset the success flag
                bool isSuccessful = true;

                try
                {
                    // Periodic update
                    if (updateScheduler.Elapsed > TimeSpan.FromSeconds(5))
                        ScheduleUpdate();

                    if (updateScheduler.IsReadyForNextAction)
                    {
                        UpdateAvailable?.Invoke(new UpdateAvailableEventArg(this));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error(ex);
                    isSuccessful = false; // If error occurred, set the success flag to false
                }

                // If the update is not successful, wait for a while before trying again so that it will not stuck the log.
                if (!isSuccessful)
                {
                    yield return 1f;
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ScheduleUpdate();
        }

        private void OnHintUpdate(object sender, PropertyChangedEventArgs ev)
        {
            if (sender is not AbstractHint hint)
                return;

            // Skip if the hint's property changed when it is hided
            if (ev.PropertyName != "Hide" && hint.Hide)
                return;

            if (hint.SyncSpeed == HintSyncSpeed.UnSync)
                return;

            float maxWaitingTime = hint.SyncSpeed switch
            {
                HintSyncSpeed.Fastest => 0,
                HintSyncSpeed.Fast => 0.1f,
                HintSyncSpeed.Normal => 0.3f,
                HintSyncSpeed.Slow => 1f,
                HintSyncSpeed.Slowest => 3f,
                _ => throw new ArgumentOutOfRangeException()
            };

            ScheduleUpdate(maxWaitingTime, hint);
        }

        private void ScheduleUpdate(float maxWaitingTime = float.MinValue, AbstractHint? updatingHint = null)
        {
            if (maxWaitingTime <= 0)
            {
                updateScheduler.Invoke();
                return;
            }

            Logger.Instance.Debug($"Scheduling update with max waiting time: {maxWaitingTime}s");
            IReadOnlyList<IReadOnlyList<AbstractHint>> allGroups = hintCollection.AllGroups;
            List<AbstractHint> predictingHints = new List<AbstractHint>();

            for (int i = 0; i < allGroups.Count; i++)
            {
                for (int j = 0; j < allGroups[i].Count; j++)
                {
                    predictingHints.Add(allGroups[i][j]);
                }
            }

            Logger.Instance.Debug($"Predicting hints count: {predictingHints.Count}");
            DateTime now = DateTime.Now;
            DateTime maxTime = now.AddSeconds(maxWaitingTime);
            DateTime delayedUpdateTime = now;

            Logger.Instance.Debug($"delayed update: {delayedUpdateTime}");
            foreach (var h in predictingHints)
            {
                if (h.SyncSpeed < updatingHint?.SyncSpeed || h == updatingHint)
                    continue;

                Logger.Instance.Debug($"Predicting hint: {h.Id} ({h.Guid})");
                DateTime estNextUpdate = h.UpdateAnalyser.EstimateNextUpdate();

                if (estNextUpdate == DateTime.MaxValue)
                    continue;

                TimeSpan delta = estNextUpdate - now;

                Logger.Instance.Debug($"Estimated next update time: {estNextUpdate} (in {delta.TotalSeconds}s)");

                // Only consider the updates that will happen within the max waiting time
                if (estNextUpdate > delayedUpdateTime && estNextUpdate < maxTime)
                    delayedUpdateTime = estNextUpdate;
            }

            Logger.Instance.Debug($"Final delayed update: {delayedUpdateTime}");
            float delay = (float)(delayedUpdateTime - now).TotalSeconds;

            // Clamp delay to maxWaitingTime
            // Increase delay by 10% to increase hit rate of prediction
            delay = Math.Min(maxWaitingTime, delay * 1.1f);

            updateScheduler.Invoke(delay, DelayType.KeepFastest);
        }

        private void StartParserTask()
        {
            lock (currentParserTaskLock)
            {
                if (currentParserTask is not null)
                    return;

                currentParserTask =
                    ConcurrentTaskDispatcher.Instance.Enqueue(async () =>
                    {
                        string richText;

                        try
                        {
                            richText = hintParser.ParseToMessage(hintCollection);

                            mainThreadDispatcher.Dispatch(() =>
                            {
                                try
                                {
                                    // If destroyed while waiting for main thread, skip the update
                                    if (this.isDestructed)
                                        return;

                                    SendHint(richText);
                                }
                                catch (Exception ex)
                                {
                                    Logger.Instance.Error(ex);
                                }
                                finally
                                {
                                    lock (currentParserTaskLock)
                                    {
                                        currentParserTask = null; // Does this in main thread
                                    }

                                    updateScheduler.Resume(); // Resume action after the parser task is finishing
                                }
                            });
                        }
                        catch (Exception ex)
                        {
                            Logger.Instance.Error(ex);

                            lock (currentParserTaskLock)
                            {
                                currentParserTask = null;
                            }

                            updateScheduler.Resume(); // Resume action if parser or main thread dispatcher failed

                            return Task.CompletedTask;
                        }

                        return Task.CompletedTask;
                    });
            }
        }

        private void SendHint(string text)
        {
            IDisplayOutput[] outputsSnapshot;

            lock (displayOutputsLock)
            {
                outputsSnapshot = displayOutputs.ToArray();
            }

            var arg = new DisplayOutputArg(this, text);
            foreach (IDisplayOutput output in outputsSnapshot)
            {
                try
                {
                    output.ShowHint(arg);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error(ex);
                }
            }
        }
    }
}
