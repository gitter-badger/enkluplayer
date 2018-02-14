using System.Net.NetworkInformation;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;

namespace CreateAR.SpirePlayer
{
	/// <summary>
	/// Waits for a state change.
	/// </summary>
    public class WaitingForConnectionApplicationState : IState
	{
        /// <summary>
        /// Messages.
        /// </summary>
	    private readonly IMessageRouter _messages;

        /// <summary>
        /// Constructor.
        /// </summary>
        public WaitingForConnectionApplicationState(IMessageRouter messages)
        {
            _messages = messages;
        }
	    
	    /// <inheritdoc cref="IState"/>
        public void Enter(object context)
	    {
	        _messages.Publish(MessageTypes.STATUS, GetNetworkSummary());
	    }

	    /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            
        }

	    /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            _messages.Publish(MessageTypes.STATUS, "");
        }

		/// <summary>
		/// Returns networking information.
		/// </summary>
		public static string GetNetworkSummary()
		{
			var builder = new StringBuilder();

#if UNITY_EDITOR || UNITY_IOS
		    builder.AppendLine("Networking information:");
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
			{
				if (nic.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
					|| nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
				{
					foreach (var ip in nic.GetIPProperties().UnicastAddresses)
					{
						if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
						{
							builder.AppendFormat("\t{0}\n", ip.Address);
						}
					}
				}
			}
#endif

#if NETFX_CORE
            builder.AppendLine("Networking information:");
            foreach (var localHostName in Windows.Networking.Connectivity.NetworkInformation.GetHostNames())
            {
                if (localHostName.IPInformation != null)
                {
                    if (localHostName.Type == Windows.Networking.HostNameType.Ipv4)
                    {
                        builder.AppendFormat("IP : {0}\n", localHostName.ToString());
                    }
                }
            }
#endif

            Log.Info(builder, builder.ToString());
			
            return builder.ToString();
		}
    }
}