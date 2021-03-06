﻿using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Simple loading UI.
    /// </summary>
    public class LoadingUIView : MonoBehaviourIUXController, ICommonLoadingView
    {
        /// <summary>
        /// The caption.
        /// </summary>
        [InjectElements("..cpn-status")]
        public TextWidget CpnStatus { get; set; }

        /// <summary>
        /// Gets/sets the status value.
        /// </summary>
        public string Status
        {
            get
            {
                return CpnStatus.Schema.Get<string>("status").Value;
            }
            set
            {
                CpnStatus.Schema.Set("status", value);
            }
        }
    }
}