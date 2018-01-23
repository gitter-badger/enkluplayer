using System.Net.NetworkInformation;
using System.Text;
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
		private string GetNetworkSummary()
		{
			var builder = new StringBuilder();
			builder.AppendLine("Networking information:");

#if UNITY_EDITOR || UNITY_IOS
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

            return builder.ToString();
		}
    }
}