using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentCreator : MonoBehaviour
{
    // Helper for Components when object Net Instantiate 
    public void Init( int type )
    {
        switch ( type)
        {
            case 0: // Basic 
                MaterialUtilities.SetMaterialToObjectsFromPool(this.gameObject, 2);
                ObjectUtilities.CreateCustomColliderFromFirstViableMesh(this.gameObject);
                ObjectUtilities.FadeInObject(gameObject, 2f);
                gameObject.AddComponent<MeshModifier>().SetSyncing(true);
                break; 
        }
        Destroy(this);
    }
}
