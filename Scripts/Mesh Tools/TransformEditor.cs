using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformEditor : MonoBehaviour
{
    /*
         IF TOOL MODE 0 IS ENABLED [TRANSFORM EDITING MODE]  ->
         Allow to edit transform of objects. ( scale, position, rotation, parenting. ) 
         > position & rotation is modificate by hand grabbing in transform modifier script. 
         > parenting can be set by create selection square (etiring with finger). 
                 Then do OK sign to parent ( it will create a parent new object, removing mesh modifier script etc. )  
                 OR   do CLOSE sign to unparenting ( and adding to each child mesh modifier script )
         > scaling can be set by pointing object with right hand   

      */

    void Update()
    {
        
    }
}
