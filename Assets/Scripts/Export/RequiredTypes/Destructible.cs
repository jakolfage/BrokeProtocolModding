using System.Collections;
using UnityEngine;

namespace BrokeProtocol.Required
{
    public sealed class Destructible : MonoBehaviour
    {
        public Transform mainT;
        public Rigidbody rb;
        private bool isServer;
        private Vector3 originalPosition;
        private Quaternion originalRotation;
        private FixedJoint fixedJoint;
        private Collider[] colliders;

        public void Initialize(bool isServer)
        {
            this.isServer = isServer;

            originalPosition = mainT.position;
            originalRotation = mainT.rotation;

            colliders = GetComponents<Collider>();

            rb.isKinematic = false;
            AddJoint();
        }

        private void AddJoint()
        {
            if (!fixedJoint)
            {
                fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.breakForce = rb.mass * 8000f;
                fixedJoint.breakTorque = rb.mass * 8000f;
            }
        }

        public void ResetProp()
        {
            mainT.position = originalPosition;
            mainT.rotation = originalRotation;

            AddJoint();

            if (isServer)
            {
                rb.isKinematic = false;
                foreach (Collider c in colliders)
                {
                    c.enabled = true;
                }
            }
        }

        private IEnumerator ResetDelay()
        {
            yield return new WaitForSeconds(60f);
            ResetProp();
        }

        private void OnJointBreak(float breakForce)
        {
            if (isServer)
            {
                rb.isKinematic = true;
                foreach (Collider c in colliders)
                {
                    c.enabled = false;
                }
            }

            StartCoroutine(ResetDelay());
        }
    }
}