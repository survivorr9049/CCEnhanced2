using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
[RequireComponent(typeof(CharacterController))]
public class CCEnhanced : MonoBehaviour
{
    public CharacterController CC;
    public LayerMask collisionMask;
    public float height;
	float _height;
	public float radius;
    public bool isGrounded;
    [Range(0.0f, 2.0f)]
    [SerializeField]float stepOffset;
    [Range(0.0f, 4.0f)]
    [SerializeField]float scanRadius;
    [Range(0.0f, 4.0f)]
    [SerializeField]float ascendMargin;
    [SerializeField]float groundDetectionMargin;
    [SerializeField]float groundDetectionSphereScale;
    float depth;
    Vector3 scanDir;
    Vector3 scanDirForward;
    Vector3 scanOrigin;
    Vector3 scanOffset;
    void Start(){
        if(CC == null) CC = GetComponent<CharacterController>();
        CC.stepOffset = 0f;
        CC.slopeLimit = 0;
        CC.height = height;
		_height = height;
		CC.radius = radius;
        CC.skinWidth = 0.01f;
    }
    void Update(){
        isGrounded = CheckGrounded();
        Step();
    }
    void LateUpdate(){
    }
    public Vector3 Lerp3D(ref Vector3 buffer, float speed){
        Vector3 difference = buffer - Vector3.Lerp(buffer, Vector3.zero, speed);
        buffer -= difference;
        return difference;
    }
    public void Move(Vector3 move){
        CC.Move(move);
    } 
    public void Step(){
        FindCastOffset();
        Vector3 scanPoint = transform.position + Vector3.up * (scanRadius - ascendMargin);
        scanOrigin = scanPoint + scanOffset;
        scanPoint = scanOrigin;
        RaycastHit hit;
        RaycastHit ground;
        if(!Physics.SphereCast(scanPoint, scanRadius, Vector3.down, out hit, _height/2, ~collisionMask)) return;
        if(CC.velocity.y > 0.1f) return;
        if(!Physics.SphereCast(transform.position, radius/2, Vector3.down, out ground, scanRadius + _height/2, ~collisionMask)) return;
        if(Vector3.Distance(ground.point, hit.point) > scanRadius) return;
        if(hit.point.y - transform.position.y > stepOffset) return;
        Vector3 stepDirection = Vector3.Scale(-transform.position + hit.point, Vector3.one - Vector3.up);
        RaycastHit step;
        if(Physics.Raycast(transform.position + stepDirection + stepDirection.normalized * 0.1f, Vector3.down, out step, _height + scanRadius, ~collisionMask))
        if(!Physics.Raycast(ground.point, step.point - ground.point, out step, Vector3.Distance(ground.point, step.point) * 1.01f, ~collisionMask)) return;
        if(Vector3.Dot(step.normal, ground.normal) > 0.95f) return;
        Debug.DrawRay(ground.point, Vector3.up, Color.green, 0.1f);
        Debug.DrawRay(transform.position - _height/2 * Vector3.up, stepDirection, Color.cyan, 0.1f);
        Debug.DrawRay(step.point, Vector3.up, Color.red, 0.1f);
        Debug.DrawRay(scanPoint, Vector3.down * _height/2, Color.blue);
        depth = Vector3.Distance(scanPoint + Vector3.down * _height/2, hit.point);
        transform.position += Vector3.up * (scanRadius - depth);
        Physics.SyncTransforms();
        if(depth > 0.025f) isGrounded = true; 
    }
    void FindCastOffset(){
        if(CC.velocity.magnitude > 0.05f){
            scanDir = Vector3.Cross(Vector3.Scale(CC.velocity,  Vector3.one - Vector3.up), Vector3.up).normalized;
            scanDirForward = Vector3.Scale(CC.velocity, Vector3.one - Vector3.up).normalized;
        }
        RaycastHit scanRight;
        RaycastHit scanForward;
        float rightOffset = 0;
        float forwardOffset = 0;
        if(Physics.SphereCast(transform.position + Vector3.up * _height/3, radius, scanDir, out scanRight, scanRadius - radius, ~collisionMask)){
            rightOffset = DistanceToPlane(scanRight.normal, scanRight.point, transform.position);
        }else if(Physics.SphereCast(transform.position + Vector3.up * _height/3, radius, -scanDir, out scanRight, scanRadius - radius, ~collisionMask)){
            rightOffset = DistanceToPlane(scanRight.normal, scanRight.point, transform.position);
        }
        if(Physics.SphereCast(transform.position + Vector3.up * _height/3, radius, scanDirForward, out scanForward, scanRadius - radius, ~collisionMask)){
            forwardOffset = DistanceToPlane(scanForward.normal, scanForward.point, transform.position);
        }else if(Physics.SphereCast(transform.position + Vector3.up * _height/3, radius, -scanDirForward, out scanForward, scanRadius - radius, ~collisionMask)){
            forwardOffset = DistanceToPlane(scanForward.normal, scanForward.point, transform.position);
        }
        scanOffset = (scanRadius - forwardOffset + 0.05f) * scanForward.normal;
        scanOffset -= scanRight.normal * Vector3.Dot(scanRight.normal, scanOffset);
        scanOffset += (scanRadius - rightOffset + 0.05f) * scanRight.normal;
 
    }
    float DistanceToPlane(Vector3 normal, Vector3 pointOnPlane, Vector3 point){
        Vector3 planeOffset = pointOnPlane - point;
		Debug.DrawRay(pointOnPlane, -planeOffset, Color.blue);
		planeOffset = planeOffset - normal * Vector3.Dot(normal, planeOffset);
        Debug.DrawRay(pointOnPlane, -planeOffset, Color.red);
        return Vector3.Distance(pointOnPlane - planeOffset, point);
    }
    public bool CheckGrounded(){
        Vector3 tip = transform.position + CC.center - Vector3.up*CC.height/2+Vector3.up*CC.radius-Vector3.up*groundDetectionMargin;
        return Physics.CheckSphere(tip, CC.radius * groundDetectionSphereScale, ~collisionMask, QueryTriggerInteraction.Ignore);
    }
    void OnDrawGizmosSelected(){
        if(CC == null) CC = GetComponent<CharacterController>();
        Vector3 tip = transform.position + CC.center - Vector3.up*CC.height/2+Vector3.up*CC.radius-Vector3.up*groundDetectionMargin;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(tip, CC.radius * groundDetectionSphereScale);
        Gizmos.DrawWireSphere(scanOrigin, scanRadius);
    }
    public bool CheckHead(){
        Vector3 tip = transform.position + CC.center + Vector3.up * CC.height/2;
        return !Physics.CheckSphere(tip, CC.radius * groundDetectionSphereScale, ~collisionMask, QueryTriggerInteraction.Ignore);
    }
    public void SetHeight(float newHeight){
        CC.height = newHeight;
		_height = newHeight;
		CC.center = Vector3.up * (_height-newHeight)/2;
    }
}