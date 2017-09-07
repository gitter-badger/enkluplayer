using System.Runtime.InteropServices;
using UnityEngine;

namespace CreateAR.Spire
{
    public class WebSocketBridge : MonoBehaviour
    {
        [DllImport("__Internal")]
        public static extern void Hello(string message);

        private void Awake()
        {
            Hello("Test");
        }
    }
}
