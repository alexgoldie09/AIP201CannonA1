using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCollider: MonoBehaviour
{
    /*
     * This class is used for creating the object's collider.
     * It is separated via type and depending on which type will
     * be used accordingly in the calculations for collisions.
     * An enum is used to create a custom data set for the type,
     * which is then made into a variable to be changed within the
     * inspector.
     */
    public enum Type
    {
        POINT,
        AXIS_ALIGNED_RECTANGLE,
        CIRCLE
    }

    public Type type;

    private void Awake()
    {
        /* 
         * Add this object to the custom physics engine list, a ternary operator is used just in case of
         * an error in referencing scene instance is null
        */
        CustomPhysicsEngine.Instance?.RegisterCollider(this);
    }

    private void OnDestroy()
    {
        /* 
         * Remove this object from the custom physics engine list, a ternary operator is used just in case of
         * an error in referencing the scene instance is null
        */
        CustomPhysicsEngine.Instance?.DeregisterCollider(this);
    }
}
