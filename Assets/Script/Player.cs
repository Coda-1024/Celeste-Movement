using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sp;
    private Animator anim;


    [Header("¡£◊”")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;

    [Header("“∆∂Ø")]
    public float speed = 10f;



    [Header("Ã¯‘æ")]
    public float jumpForce = 10f;
    public bool betterJump = true;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public bool isJump;
    public bool grounded = true;

    //…‰œﬂºÏ≤‚∑¢…‰Œª÷√
    [Header("µÿ√Ê")]
    public Transform leftFootPos;
    public Transform rightFootPos;
    public bool onGround;
    public float timeRecord = -100;

    //∑∂ŒßºÏ≤‚«Ω±⁄
    [Header("«Ω")]
    public Transform leftHandPos;
    public Transform rightHandPos;
    public bool onWall;
    public float slideSpeed = 3f;
    public bool leftWall;

    [Header("≈ ≈¿")]
    public float grabSpeed = 10f;
    public bool isGrab;


    [Header("ÃÂ¡¶")]
    public float power = 100f;
    public float grabCost = 10f;
    public float clambCost = 40f;

    [Header("≥Â¥Ã")]
    public float dashForce = 100f;
    public bool isDash;
    public bool canDash = true;
    public float dashTime = 0.1f;
    public float dashStart;

    [Header("Õœ”∞")]
    public Transform ghosts;
    public Color ghostColor;
    public float ghostInterval;
    private Sequence s;


    [Header(" ßøÿ")]
    public bool withoutControll;
    public float freezeTime = 0.5f;
    public float freezeRecord;

    [Header("£ø")]
    public GameObject hh;



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
       
      
    }


    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(x, y);


        if (dir.x < 0 && !onWall)
        {
            sp.flipX = true;
        }
        else if (dir.x > 0 && !onWall)
        {
            sp.flipX = false;
        }


        #region “∆∂Ø
        DetectFreezeTime();
        DetectDashTime();
        if (!withoutControll && !isDash)
        {
            Walk(dir);
        }

        if(onGround && rb.velocity.x != 0)
        {
            anim.SetBool("isRun", true);
        }
        else
        {
            anim.SetBool("isRun", false);
        }

        #endregion

        #region Ã¯‘æ
        DetectGround();
        if (Input.GetKeyDown(KeyCode.C))
        {   
            if(onGround)
            {
               Jump();
               
            }
            else if(onWall)
            {
                JumpOnwall(x);
            }
            else
            {
                timeRecord = Time.time;
            }
            
        }

        if(betterJump && !isDash)
        {
            BetterJump();
        }

        if(rb.velocity.y <= 0)
        {
            isJump = false;
        }

        if(rb.velocity.y > 0 && !onWall)
        {
            anim.SetBool("isJump", true);
        }
        else
        {
            anim.SetBool("isJump", false);
        }

        if(rb.velocity.y < 0 && !onGround)
        {
            anim.SetBool("isFall", true);
        }
        else
        {
            anim.SetBool("isFall", false);
        }

        //¬‰µÿ¡£◊”–ßπ˚
        if (rb.velocity.y > 0)
        {
            grounded = false;
        } 
        if(onGround && !grounded)
        {
            jumpParticle.Play();
            grounded = true;
        }
        #endregion


        #region ≈ ≈¿
        DetectWall();
        if (onWall && !isJump && xRaw != 0 && !isGrab && !isDash) 
        {
            Slide();
        }


        if (Input.GetKey(KeyCode.Z) && onWall && power > 0)
        {
            isGrab = true;
            
            if (!withoutControll && !isDash)
            {
                rb.gravityScale = 0f;
                Grab(y);
            }
         
        }


        if (Input.GetKeyUp(KeyCode.Z) || power <= 0 || !onWall || isDash )
        {
            rb.gravityScale = 3f;
            if (isDash)
            {
                if(y == 0)
                {
                    rb.gravityScale = 0f;
                }
            }

           
            isGrab = false;
        }

        if(onWall && !onGround)
        {
            anim.SetBool("isClimb", true);
            anim.SetBool("isDashClimb", true);
        }
        else
        {
            anim.SetBool("isClimb", false);
            anim.SetBool("isDashClimb", false);
        }

        if(isGrab && y == 0 && !onGround)
        {
            anim.SetBool("isGrab", true);
        }
        else
        {
            anim.SetBool("isGrab", false);
        }

        #endregion

        #region ≥Â¥Ã
        if (Input.GetKeyDown(KeyCode.X) && canDash && !isDash)
        {
            dash(new Vector2(xRaw, yRaw));
        }

        if(!canDash)
        {
            anim.SetBool("isDash", true);
        }
        else
        {
            anim.SetBool("isDash", false);
        }
        #endregion

    }



    void Walk(Vector2 direct)
    {
        rb.velocity = new Vector2(direct.x * speed, rb.velocity.y);
    }



    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);

        rb.velocity += Vector2.up * jumpForce;

        isJump = true;

        jumpParticle.Play();
    }

   
    void JumpOnwall(float x)
    {

        FreezeTime();

        rb.velocity = Vector2.zero;

        if (leftWall)
        {   if(isGrab && x <= 0)
            {
                power -= 30;
                rb.velocity += Vector2.up * jumpForce;
            }
            else
            {
                rb.velocity += new Vector2(1, 1).normalized * jumpForce;
                sp.flipX = false;
            }
          
        }
        else
        {
            if(isGrab && x >= 0 )
            {
                power -= 30;
                rb.velocity += Vector2.up * jumpForce;
            }
            else
            {
                rb.velocity += new Vector2(-1, 1).normalized * jumpForce;
                sp.flipX = true;
            }
         
        }

        isJump = true;

       
    }

    void BetterJump()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;

        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.C))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void DetectGround()
    {

        RaycastHit2D leftHit = Physics2D.Raycast(leftFootPos.position, Vector2.down, 0.1f, 1 << LayerMask.NameToLayer("Wall"));
        RaycastHit2D rightHit = Physics2D.Raycast(rightFootPos.position, Vector2.down, 0.1f, 1 << LayerMask.NameToLayer("Wall"));

        if (leftHit || rightHit)
        {
            power = 100f;
            if (!isDash)
                canDash = true;
            //Ã¯‘æ÷°ª∫≥Â
            if (Time.time - timeRecord  <= 0.06f)
            {
                Jump();
                timeRecord = 0f;
            }
            else
            {
                onGround = true;
            }
        }
        else
        {
            onGround = false;
        }
    }

    void DetectWall()
    {
        Collider2D lefthandColl = Physics2D.OverlapCircle(leftHandPos.position, .05f, 1 << LayerMask.NameToLayer("Wall"));
        Collider2D rightHandColl = Physics2D.OverlapCircle(rightHandPos.position, .05f, 1 << LayerMask.NameToLayer("Wall"));

        if(lefthandColl != null || rightHandColl != null)
        {
            onWall = true;

            if(lefthandColl != null)
            {
                leftWall = true;
            }
            else
            {
                leftWall = false;
            }
        }
        else
        {
            onWall = false;
        }
    }

    //œ¬ª¨
    void Slide()
    {
        rb.velocity = new Vector2(rb.velocity.x,  -slideSpeed);
    }

    //≈ ≈¿
    void Grab(float y)
    {
        rb.velocity = new Vector2(0, y * grabSpeed);
        if (y > 0)
        {
            power -= Time.deltaTime * clambCost;
        }
        else
        {
            power -= Time.deltaTime * grabCost;
        }
        
    }


    void FreezeTime()
    {
        withoutControll = true;
        freezeRecord = Time.time;
    }

    void DetectFreezeTime()
    {
        if(Time.time - freezeRecord >= freezeTime)
        {
            withoutControll = false;
        }
    }

    //≥Â¥Ã
    void dash(Vector2 dir)
    {
        ShowGhost();
        BeginDashTime();
        canDash = false;
        rb.velocity = Vector2.zero;

        if(dir == Vector2.zero)
        {
            dir = sp.flipX ? Vector2.left : Vector2.right;
        }

        rb.velocity += dir.normalized * dashForce;

    }

    void ShowGhost()
    {
        s = DOTween.Sequence();
        for (int i = 0; i < ghosts.childCount; i++)
        {
            Transform ghost = ghosts.GetChild(i);
            SpriteRenderer ghostSp = ghost.GetComponent<SpriteRenderer>();
            s.AppendCallback(() =>
            {
                ghost.position = transform.position;
                ghostSp.sprite = sp.sprite;
                ghostSp.flipX = sp.flipX;

                ghostSp.material.DOColor(ghostColor, 0);

                ghostSp.material.DOColor(Color.clear, .5f);


            });
            s.AppendInterval(ghostInterval);

        }
    }

    void DetectDashTime()
    {
        if(isDash && Time.time - dashStart >= dashTime)
        {
            rb.velocity = Vector2.zero;
            isDash = false;
            if(rb.gravityScale == 0 && !isGrab)
            {
                rb.gravityScale = 3f; 
            }
        }
    }

    void BeginDashTime()
    {
        isDash = true;
        dashStart = Time.time;
        dashParticle.Play();
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(leftFootPos.position, leftFootPos.position + Vector3.down * 0.1f);
        Gizmos.DrawLine(rightFootPos.position, rightFootPos.position + Vector3.down * 0.1f);

        Gizmos.DrawWireSphere(leftHandPos.position, 0.05f);
        Gizmos.DrawWireSphere(rightHandPos.position, 0.05f);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("hh"))
        {
            hh.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("hh"))
        {
            hh.SetActive(false);
        }
    }

}



