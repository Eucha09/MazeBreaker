using UnityEngine;

public class GrapplingGun : MonoBehaviour
{

    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, player;
    private float maxDistance = 100f;
    private SpringJoint joint;

    public bool IsGrappleDone { get; set; }
    public bool IsGrappleSuccess { get; set; }

    /// <summary>
    /// Call whenever we want to start a grapple
    /// </summary>
    public void StartGrapple()
    {
        Debug.Log("Grapple Start");
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // 마우스 위치에서 레이를 발사
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance, whatIsGrappleable))
        {



            IsGrappleSuccess = true;
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            //The distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            //Adjust these values to fit your game.
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;
        }
        else
        {
            IsGrappleSuccess = false;
        }
    }


    /// <summary>
    /// Call whenever we want to stop a grapple
    /// </summary>
    public void StopGrapple()
    {
        IsGrappleDone = false;
        Destroy(joint);
    }

    public void GrappleDone()
    {
        IsGrappleDone = true;
    }



    public bool IsGrappling()
    {
        return joint != null;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}