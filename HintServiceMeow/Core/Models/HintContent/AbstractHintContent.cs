﻿using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Tools;
using System;

namespace HintServiceMeow.Core.Models.HintContent
{
    public abstract class AbstractHintContent
    {
        public delegate void UpdateHandler();

        public event UpdateHandler ContentUpdated;

        public void OnUpdated()
        {
            try
            {
                ContentUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                LogTool.Error(ex);
            }
        }

        public abstract void TryUpdate(AbstractHint.TextUpdateArg ev);

        public abstract string GetText();
    }
}
