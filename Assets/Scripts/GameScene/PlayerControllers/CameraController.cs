using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;


namespace GameScene.PlayerControllers
{
    /**
 * <summary>
 *      The controller for the <see cref="Camera"/> bound to the player in the game
 * </summary>
 */
    public class CameraController : MonoBehaviour
    {
        /**
     * <value>
     *      the local offset of the camera
     * </value>
     */
        [SerializeField] private Vector3 baseOffset = new Vector3(.5f, .3f, -2.2f);

        private Vector3 offset;

        public Vector3 Offset => offset;

        /**
     * <value>
     *      the base angle on the x-axis (pitch) of the camera
     * </value>
     */
        [SerializeField] private float baseAngle = -5F;

        /**
     * <value>
     *      the max angle of the camera on the x-axis (pitch)
     * </value>
     * <para>
     *      Example:
     *      <code>
     *          maxAngle = 30f;
     *          the pitch of the camera can go from 
     *      </code>
     * </para>
     */
        [SerializeField] private float maxAngle = 90F;

        private float _addedAngle;

        /**
     * <value>
     *      the angle of the camera on the local x-axis (pitch)
     * </value>
     */
        public float AddedAngle
        {
            get => _addedAngle;
            set
            {
                if (Mathf.Abs(value) <= maxAngle)
                {
                    _addedAngle = value;
                }
                else if (value < 0)
                {
                    _addedAngle = -maxAngle;
                }
                else
                {
                    _addedAngle = maxAngle;
                }
            }
        }


        /**
     * <value>
     *      the attached <see cref="Camera"/>
     * </value>
     */
        private Camera cam;

        public Camera Camera => cam;


        void Start()
        {
            ResetOffset();
            cam = GetComponent<Camera>();
            _addedAngle = baseAngle;
        }


        /**
     * <summary>
     *      called when the player moves
     * </summary>
     */
        public void OnPlayerMove(Vector3 camAnchor, Transform playerTransform)
        {
            var camTransform = cam.transform;

            Vector3 computedOffset = Quaternion.AngleAxis(AddedAngle, Vector3.right) * offset;
            Vector3 camPosition = camAnchor + playerTransform.TransformVector(computedOffset);
            camTransform.position = camPosition;

            Vector3 playerAngles = playerTransform.localEulerAngles;

            Vector3 camAngles = playerAngles + Vector3.right * AddedAngle;

            camTransform.localEulerAngles = camAngles;
        }

        public void ChangeOffset(Vector3 offset)
        {
            this.offset = offset;
        }

        public void ResetOffset()
        {
            this.offset = baseOffset;
        }
    }
}