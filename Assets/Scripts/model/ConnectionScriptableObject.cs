using UnityEngine;

namespace model
{
    [CreateAssetMenu(fileName = "ConnectionData", menuName = "Rukmaksii/ConnectionScriptableObject")]
    public class ConnectionScriptableObject : ScriptableObject
    {
        public ConnectionData Data = new ConnectionData();
    }
}