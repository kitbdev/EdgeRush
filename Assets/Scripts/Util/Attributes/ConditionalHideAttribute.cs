using UnityEngine;
using System;
using System.Collections;

//Original version of the ConditionalHideAttribute created by Brecht Lecluyse (www.brechtos.com)
//Modified by: Sebastian Lague

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute {
    public string conditionalSourceField;
    public bool showIfTrue = true;
    public int[] enumIndices = new int[0];

    public ConditionalHideAttribute(string boolVariableName, bool showIfTrue) {
        conditionalSourceField = boolVariableName;
        this.showIfTrue = showIfTrue;
    }
    // todo multiple bools
    // public ConditionalHideAttribute(bool showIfTrueparams, bool And, string boolVariableNames) {
    //     conditionalSourceField = boolVariableName;
    //     this.showIfTrue = showIfTrue;
    // }

    public ConditionalHideAttribute(string enumVariableName, params int[] enumIndices) {
        conditionalSourceField = enumVariableName;
        this.enumIndices = enumIndices;
        this.showIfTrue = true;
    }
    public ConditionalHideAttribute(string enumVariableName, bool showIfTrue = true, params int[] enumIndices) {
        conditionalSourceField = enumVariableName;
        this.enumIndices = enumIndices;
        this.showIfTrue = showIfTrue;
    }

}



