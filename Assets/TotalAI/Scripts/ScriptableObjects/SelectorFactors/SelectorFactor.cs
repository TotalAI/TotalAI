using UnityEngine;

namespace TotalAI
{
    public abstract class SelectorFactor : ScriptableObject
    {
        // Provide a description in the SelectorFactor inspector
        public string editorDescription; 
        public MinMaxCurve minMaxCurve;

        public abstract float Evaluate(Selector selector, Agent agent, Mapping mapping, out bool overrideValue);
    }
}