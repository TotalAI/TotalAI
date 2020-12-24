using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TotalAI
{
    public abstract class ChoicesType : ScriptableObject
    {
        public abstract int NumberChoices(Agent agent);
        public abstract string[] OptionNames(Agent agent);
        public abstract List<T> GetChoices<T>(Agent agent);
        public abstract Type ForType(Agent agent);
    }
}

