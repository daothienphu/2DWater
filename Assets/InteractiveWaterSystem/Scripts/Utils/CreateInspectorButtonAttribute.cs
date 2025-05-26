using System;
using UnityEngine;

namespace Utils
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CreateInspectorButtonAttribute : PropertyAttribute
    {
        public string ButtonLabel { get; }
        
        //This will create a single Button in the inspector, used for quick testing or debugging. Will not work for multiple buttons.
        public CreateInspectorButtonAttribute(string buttonLabel = null)
        {
            ButtonLabel = buttonLabel;
        }
    }
}