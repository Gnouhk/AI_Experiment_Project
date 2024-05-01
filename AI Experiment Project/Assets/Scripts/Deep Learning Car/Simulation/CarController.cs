using UnityEngine;

public class CarController : MonoBehaviour
{
    #region Members

    // Used for unique ID generation
    private static int idGenerator = 0;

    // Returns the next unique id in the sequence.
    private static int NextID { get { return idGenerator++; } }

    #endregion


}
