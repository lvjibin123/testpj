#define DEBUG_CC2D_RAYS
using UnityEngine;
using System;
using System.Collections.Generic;


public class CharacterController2D : MonoBehaviour
{

	#region events, properties and fields

	public event Action<RaycastHit2D> onControllerCollidedEvent;
	public event Action<Collider2D> onTriggerEnterEvent;
	public event Action<Collider2D> onTriggerStayEvent;
	public event Action<Collider2D> onTriggerExitEvent;

	[SerializeField]
	[Range( 0.001f, 0.3f )]
	float _skinWidth = 0.02f;
    List<Vector3> rayorigin;

	/// <summary>
	/// defines how far in from the edges of the collider rays are cast from. If cast with a 0 extent it will often result in ray hits that are
	/// not desired (for example a foot collider casting horizontally from directly on the surface can result in a hit)
	/// </summary>
	public float skinWidth
	{
		get { return _skinWidth; }
		set
		{
			_skinWidth = value;
		}
	}


	/// <summary>
	/// mask with all layers that the player should interact with
	/// </summary>
	public LayerMask platformMask = 0;

	/// <summary>
	/// mask with all layers that should act as one-way platforms. Note that one-way platforms should always be EdgeCollider2Ds. This is because it does not support being
	/// updated anytime outside of the inspector for now.
	/// </summary>
	[SerializeField]
	LayerMask oneWayPlatformMask = 0;

    bool isEventActive;


	[HideInInspector][NonSerialized]
	public new Transform transform;
	[HideInInspector][NonSerialized]

	#endregion

	/// <summary>
	/// stores our raycast hit during movement
	/// </summary>
	RaycastHit2D _raycastHit;

	/// <summary>
	/// stores any raycast hits that occur this frame. we have to store them in case we get a hit moving
	/// horizontally and vertically so that we can send the events after all collision state is set
	/// </summary>
	List<RaycastHit2D> _raycastHitsThisFrame = new List<RaycastHit2D>();

	#region Monobehaviour

	void Awake()
	{
		// add our one-way platforms to our normal platform mask so that we can land on them from above
		platformMask |= oneWayPlatformMask;

		// cache some components
		transform = GetComponent<Transform>();

		// here, we trigger our properties that have setters with bodies
		skinWidth = _skinWidth;

	}

    public void setEventActive(bool isEventActive) {
        this.isEventActive = isEventActive;
    }

	public void OnTriggerEnter2D( Collider2D col )
	{
        if (onTriggerEnterEvent != null)
			onTriggerEnterEvent( col );
	}


	public void OnTriggerStay2D( Collider2D col )
	{
        if (onTriggerStayEvent != null)
			onTriggerStayEvent( col );
	}


	public void OnTriggerExit2D( Collider2D col )
	{
        if (onTriggerExitEvent != null)
			onTriggerExitEvent( col );
	}

	#endregion


    public void move(Vector3 deltaMovement)
    {
        _raycastHitsThisFrame.Clear();

        if (_raycastHitsThisFrame.Count == 0)
            moveDir(ref deltaMovement);


        if (onControllerCollidedEvent != null && isEventActive)
        {
            if (_raycastHitsThisFrame.Count > 0)
            {
                onControllerCollidedEvent(_raycastHitsThisFrame[0]);
            }
            else
            {
                transform.Translate(deltaMovement, Space.World);
            }
        }
    }


	#region Movement Methods


    void moveDir(ref Vector3 deltaMovement)
    {
        var rayDistance = Mathf.Sqrt(deltaMovement.x*deltaMovement.x+deltaMovement.y*deltaMovement.y) + _skinWidth;
        float m = 0, b = 0, a = 0;
        if(deltaMovement.x == 0)
        {
            b = 0;
            a = deltaMovement.y;
        }
        else 
        {
            m = deltaMovement.y / deltaMovement.x;
            b = Mathf.Sqrt(0.1f * 0.1f / (m * m + 1));
            if (deltaMovement.x < 0)
                b = -b;
            a = b * m;
        }

        Vector3 initialRayOrigin = transform.position + new Vector3(b, a, 0);
        Vector3 initialRayOrigin1 = transform.position + new Vector3(a, -b, 0);
        Vector3 initialRayOrigin2 = transform.position + new Vector3(-a, b, 0);

        rayorigin = new List<Vector3>();
        rayorigin.Add(initialRayOrigin);
        rayorigin.Add(initialRayOrigin1);
        rayorigin.Add(initialRayOrigin2);

        float rayPointD = 1;
        RaycastHit2D _raycastTmp = _raycastHit;
	    Vector3 deltaDTmp = deltaMovement;

        for (var i = 0; i < 3; i++)
        {
            var ray = new Vector2(rayorigin[i].x, rayorigin[i].y);
            Vector3 deltaD = new Vector3(b, a, 0) * 10 * rayDistance;
            if (i != 0)
            {
                deltaD = new Vector3(b, a, 0) * 10 * (rayDistance + 0.1f);
                _raycastHit = Physics2D.Raycast(ray, deltaMovement, (rayDistance + 0.1f), platformMask);
            }
            else {
                _raycastHit = Physics2D.Raycast(ray, deltaMovement, rayDistance, platformMask);
            }
            //Debug.DrawRay(rayorigin[i], deltaD, Color.red, 2, false);

            if (_raycastHit)
            {
                if (_raycastHit.transform.name.Contains("brick"))
                {
                    if (Mathf.Abs(_raycastHit.distance) < rayPointD)
                    {
                        rayPointD = Mathf.Abs(_raycastHit.distance);
                        _raycastTmp = _raycastHit;
                        //deltaDTmp = deltaD;
                        deltaDTmp = (new Vector3(b, a, 0) * 10 * rayPointD);
                    }
                }else{
                    //方块、球
                    _raycastHitsThisFrame.Add(_raycastHit);
                    break;
                }
            }
        }
        //用最短的那个射线
        if (rayPointD < 1)
        {
            float xx = 0, yy = 0;
            if (_raycastTmp.normal.x == 0)
            {
                if (_raycastTmp.normal.y < 0)
                {
                    //方块在上
                    yy = _raycastTmp.point.y - transform.position.y - 0.11f;
                }
                else
                {
                    //方块在下
                    yy = _raycastTmp.point.y - transform.position.y + 0.11f;
                }

                xx = yy * deltaDTmp.x / deltaDTmp.y;
            }
            else if (_raycastTmp.normal.y == 0)
            //else
            {
                if (_raycastTmp.normal.x < 0)
                {
                    //方块在右
                    xx = _raycastTmp.point.x - transform.position.x - 0.11f;
                }
                else
                {
                    //方块在左
                    xx = _raycastTmp.point.x - transform.position.x + 0.11f;
                }
                if (deltaDTmp.x == 0)
                    yy = deltaDTmp.y;
                else
                    yy = xx * deltaDTmp.y / deltaDTmp.x;
            }
            else
            {
                float distance = _raycastTmp.point.x - transform.position.x;
                if (_raycastTmp.normal.x > 0)
                {
                    if ((_raycastTmp.normal.y > 0 && transform.position.x > _raycastTmp.point.x && transform.position.y > _raycastTmp.point.y)
                        || (_raycastTmp.normal.y < 0 && transform.position.x > _raycastTmp.point.x && transform.position.y < _raycastTmp.point.y))
                    {
                        //右上斜面, 右下斜面
                        calcDeltaXY2(ref xx, ref yy, deltaDTmp, rayPointD);
                    }
                    else
                    {
                        xx = deltaDTmp.x;
                        yy = deltaDTmp.y;
                    }
                }
                else
                {
                    if ((_raycastTmp.normal.y > 0 && transform.position.x < _raycastTmp.point.x && transform.position.y > _raycastTmp.point.y)
                        || (_raycastTmp.normal.y < 0 && transform.position.x < _raycastTmp.point.x && transform.position.y < _raycastTmp.point.y))
                    {
                        //左上斜面, 左下斜面
                        calcDeltaXY2(ref xx, ref yy, deltaDTmp, rayPointD);
                    }
                    else
                    {
                        xx = deltaDTmp.x;
                        yy = deltaDTmp.y;
                    }
                }
            }

            transform.position = new Vector3(transform.position.x + xx,
                transform.position.y + yy,
                transform.position.z);

            _raycastHitsThisFrame.Add(_raycastTmp);
        }
        rayorigin.Clear();

    }

    void calcDeltaXY(ref float xx, ref float yy, Vector3 deltaDTmp, float distance, float length) {
        xx = (distance + length) / 2;
        if (Mathf.Abs(xx) > deltaDTmp.x)
        {
            xx = deltaDTmp.x;
            yy = deltaDTmp.y;
        }
        else
        {
            if (xx * deltaDTmp.x < 0)
                xx = -xx;
            if (deltaDTmp.x == 0)
                yy = deltaDTmp.y;
            else
                yy = xx * deltaDTmp.y / deltaDTmp.x;
        }
    }

    // 利用角度计算碰撞时的位置
    void calcDeltaXY2(ref float xx, ref float yy, Vector3 deltaDTmp, float rayPointD)
    {
        float angel = Mathf.Atan2(deltaDTmp.x, deltaDTmp.y) * Mathf.Rad2Deg;
        float angel2 = 0;
        if (Mathf.Abs(deltaDTmp.x) > Mathf.Abs(deltaDTmp.y))
        {
            angel2 = Mathf.Atan2(Mathf.Abs(deltaDTmp.y), Mathf.Abs(deltaDTmp.x)) * Mathf.Rad2Deg;
        }
        else {
            angel2 = Mathf.Atan2(Mathf.Abs(deltaDTmp.x), Mathf.Abs(deltaDTmp.y)) * Mathf.Rad2Deg;        
        }
        float realAngel = (angel2 + 45) / 2;
        float cutD = 0.1f * Mathf.Tan(realAngel / Mathf.Rad2Deg);
        float moveD = rayPointD - cutD;
        xx = moveD * Mathf.Sin(angel / Mathf.Rad2Deg);
        if (deltaDTmp.x == 0)
            yy = rayPointD;
        else 
            yy = xx * deltaDTmp.y / deltaDTmp.x;
    }

	#endregion

}
