using model;
using UnityEngine;

namespace GameManagers
{
    
    [CreateAssetMenu(fileName = "ConnectionData", menuName = "Rukmaksii/ConnectionScriptableObject")]
    public class ConnectionScriptableObject : ScriptableObject
    {
        public ConnectionData Data = null;
    }
}