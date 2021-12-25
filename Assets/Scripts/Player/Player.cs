using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using Mirror;

public class Player : NetworkBehaviour
{
    [Header("Loadout")]
    [SerializeField] protected Gun Primary;
    [SerializeField] protected Gun Secondary;

    [Header("Main Objects")]
    [SerializeField] protected Transform PlayerBody;
    [SerializeField] protected CharacterController cc;
    [SerializeField] protected Transform Camera;
    [SerializeField] protected Camera Cam;

    [Header("Player Settings")]
    [SerializeField] protected float Gravity;
    [SerializeField] protected float CrouchSpeed;
    protected bool isCrouching = false;
    [SerializeField] protected float WalkSpeed;
    [SerializeField] protected float RunSpeed;
    [SerializeField] protected float DownedSpeed;
    [SerializeField] protected float JumpPower;
    public float MouseSens;

    private DynamicCrosshair dc;

    [Header("Utility")]
    [SerializeField] protected Util Utility = Util.None;
    public GameObject Flashlight;
    public GameObject NVVolume;

    [SyncVar]
    protected string playerName;

    protected bool canUseUtil = true;

    [SyncVar]
    protected bool ToggledUtil = false;
    protected enum Util {None,Flashlight, NV,};

    [Header("Shooting / Weapon")]
    [SerializeField] protected GameObject BulletTracer;

    protected Gun CurrentGun = null;
    protected float gunTimer = 0;

    protected GameObject Viewmodel;

    // PRIMARY GUN AMMO
    [SyncVar]
    protected int p_Mag;
    [SyncVar]
    protected int p_MaxAmmo;

    [Header("Slide")]
    [SerializeField] protected float slideSpeed;
    [SerializeField] protected float slideTime;

    [Header("Ping")]
    [SerializeField] protected GameObject PingObj;

    [Header("UI")]
    public GameObject PauseMenu;
    public GameObject HUD;
    public Image HealthBar;
    public Text HealthText;
    public Image ShieldBar;
    public Text ShieldText;
    public Image DownedRing;
    public Text pingText;

    [Header("protected Script Settings")]
    protected bool isGrounded;
    protected float airSpeed;

    [SyncVar(hook = nameof(UpdateHealth))]
    protected float Health;

    protected float DownedHealth;

    [SyncVar(hook = nameof(UpdateShield))]
    protected float Shield;

    [SyncVar]
    protected bool isDowned;
    [SyncVar]
    protected bool isDead;
    [SyncVar]
    private bool canMove = true;

    //Sliding
    protected bool isSliding;
    protected Vector3 slideDir;
    protected float timePassed;

    //Sprinting
    protected bool isRunning;
    protected float currentSpeed;
    protected bool Paused;

    protected GameManager gm;
    protected InputManager im;

    public void setName(string n)
    {
        playerName = n;
    }
    protected void Start()
    {
        if (netIdentity.isLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            HUD.SetActive(true);
            Cam.gameObject.SetActive(true);

            GameSettings.current.onSettingsChanged += onSettingsChanged;
            GameSettings.PlayerSettings ps = GameSettings.current.GetSettings();
            MouseSens = (float)ps.Sensitivty;

            // Making Player Body In-Visible
            showBody(false);

            gm = GameManager.self;

            clientStart();

            Camera.gameObject.SetActive(true);

            dc = GetComponentInChildren<DynamicCrosshair>();
            im = InputManager.current;
            getInputManager();
        }
        else
        {
            Cam.gameObject.SetActive(false);
            HUD.SetActive(false);
            showBody(true);
            Camera.gameObject.SetActive(false);
        }

        if (netIdentity.isServer)
        {
            isDowned = false;
            DownedHealth = 100;
            Health = 100;
            Shield = 50;
            gm = GameManager.self;
            serverStart();
            canMove = true;
        }

        /*
         * UPDATE AMMO HERE
        if (netIdentity.isServer)
        {
            Primary.Init();
            Debug.Log(Primary.Mag());

            Secondary.Init();
            Debug.Log(Secondary.Mag());
        }
        */
    }

    public void showBody(bool v)
    {
        PlayerBody.gameObject.SetActive(v);
        for (int i = 0; i < PlayerBody.transform.childCount; i++)
        {
            PlayerBody.transform.GetChild(i).gameObject.SetActive(v);
        }
    }

    [Server]
    protected virtual void serverStart() { }

    [Client]
    protected virtual void clientStart() { }

    protected void onSettingsChanged(GameSettings.PlayerSettings ps)
    {
        MouseSens = (float)ps.Sensitivty;
    }

    [Server]
    public void changeCanMove(bool v)
    {
        //Debug.Log("Cannot Move");
        canMove = v;
    }

    private void getInputManager()
    {
        // subscribe to on escape and jump events
        im.onEscape += onEscape;
        im.onJump += onJump;
    }

    [Client]
    protected virtual void onClientUpdate() { }
    [Server]
    protected virtual void onServerUpdate() { }

    private float getBloom()
    {
        if (!isGrounded)
        {
            return CurrentGun.AirBloom;
        }
        else if (isSliding)
        {
            return CurrentGun.SlideBloom;
        }
        else if (isCrouching)
        {
            return CurrentGun.CrouchBloom;
        }
        else if (isRunning && im.getMovement().y > 0)
        {
            return CurrentGun.RunBloom;
        }
        else
        {
            return CurrentGun.BaseBloom;
        }
    }


    protected float lastPressed = 0;
    protected void Update()
    {
        if (netIdentity.isLocalPlayer && !Paused)
        {
            pingText.text = $"{Mathf.Round((float)NetworkTime.rtt * 1000)}ms";
            UpdateGravity();
            if(canMove) UpdateButtons();
            //if(Input.GetKeyDown(KeyCode.E)) CmdInteract();
            if (isDowned) UpdateDownedHealth();
            if (CurrentGun != null)
            {
                dc.setActive(true);
                dc.setSize(getBloom());
            }
            else
            {
                dc.setActive(false);
            }
        }

        if (netIdentity.isLocalPlayer)
        {
            onClientUpdate();
            //Debug.Log("CLIENT: " + gunTimer);
        }

        if (netIdentity.isServer)
        {
            ServerUpdate();
            onServerUpdate();
        }
    }

    protected void onJump(bool active)
    {
        Debug.Log("JUMP EVENT CALLED : " + active);
        if (active && isGrounded && !isSliding && !isDowned && canMove)
        {
            airSpeed = Mathf.Sqrt((JumpPower / 100) * -2f * -Gravity);
            cc.Move(Vector3.Lerp(cc.transform.position, Vector3.up * airSpeed,Time.fixedDeltaTime*2));
        }
    }

    protected void onEscape(bool active)
    {
        Debug.Log("ESC EVENT CALLED : " + active);
        if (active && netIdentity.isLocalPlayer)
        {
            lastPressed = Time.time;
            Paused = !Paused;
            PauseMenu.SetActive(Paused);
            HUD.SetActive(!Paused);

            if (Paused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    protected void FixedUpdate()
    {
        if (netIdentity.isLocalPlayer && !Paused)
        {
            Movement();
            CameraMovement();
        }
    }

    private bool hasfullyDied = false;
    private string lasttookdmgfrom;

    [Server]
    protected void ServerUpdate()
    {
        //Debug.Log("SERVER: " + gunTimer);

        if (isDowned)
        {
            DownedHealth -= Time.deltaTime * 1.8f;
            if(DownedHealth <= 0 && hasfullyDied)
            {
               //Debug.Log("Dead!");

                if (!hasfullyDied)
                {
                    gm.onKilled(lasttookdmgfrom,cc.gameObject.name);
                }

                hasfullyDied = true;
            }
        }

        gunTimer -= Time.deltaTime;
    }

    [Command]
    protected void CmdToggleUtility()
    {
        if (!canUseUtil) return;
        if (ToggledUtil)
        {
            if(Utility == Util.NV)
            {
                RpcToggleNV(false);
            }
            else if (Utility == Util.Flashlight)
            {
                // Turn Off FlashLight
                RpcUpdateFlashlight(false);
            }
        }
        else
        {
            if (Utility == Util.NV)
            {
                RpcToggleNV(true);
            }
            else if(Utility == Util.Flashlight)
            {
                // Turn On FlashLight
                RpcUpdateFlashlight(true);
            }
        }

        ToggledUtil = !ToggledUtil;
    }

    [ClientRpc(includeOwner = true)]
    protected void RpcUpdateFlashlight(bool act)
    {
        if(Flashlight != null)
        {
            Flashlight.SetActive(act);
        }
    }

    [TargetRpc]
    protected void RpcToggleNV(bool nvon)
    {
        // Replace Basic Renderer With NV Renderer
        //  Basic Renderer Index: 0
        //  NV Shader Renderer Index: 1
        if (NVVolume == null) return;
        var c_Data = Cam.GetUniversalAdditionalCameraData();
        if (nvon)
        {
            // Turn On NV's
            c_Data.SetRenderer(1);
            NVVolume.SetActive(true);
        }
        else
        {
            // Turn Off NV's
            c_Data.SetRenderer(0);
            NVVolume.SetActive(false);
        }
    }

    protected Gun GetGun(int gID)
    {
        Gun[] objects = Resources.LoadAll<Gun>("Weapons/WeaponSO");
        foreach (Gun g in objects)
        {
            if (g.id == gID)
            {
                return g;
            }
        }
        return null;
    }

    [Command]
    protected void CmdEquipGun(int gID)
    {
        CurrentGun = GetGun(gID);
        RpcOnEquip(gID);
    }

    [TargetRpc]
    protected void RpcOnEquip(int gID)
    {
        CurrentGun = GetGun(gID);

        if (Viewmodel != null)
        {
            Destroy(Viewmodel);
        }

        Viewmodel = ShowViewmodel(CurrentGun, Camera,Cam);
    }

    [Command]
    protected void CmdDeEquip()
    {
        CurrentGun = null;
        RpcOnDeEquip();
    }

    [TargetRpc]
    protected void RpcOnDeEquip()
    {
        CurrentGun = null;
        if (Viewmodel != null)
        {
            Destroy(Viewmodel);
        }
    }

    [Client]
    protected GameObject ShowViewmodel(Gun g,Transform Parent,Camera cam)
    {
        GameObject newwep = Instantiate(g.Viewmodel, Parent) as GameObject;
        newwep.transform.localPosition = Vector3.zero;
        newwep.transform.localRotation = Quaternion.identity;
        newwep.transform.localScale = new Vector3(0.05f,0.05f,0.05f);

        Camera c = newwep.GetComponentInChildren<Camera>();

        var cameraData = cam.GetUniversalAdditionalCameraData();
        cameraData.cameraStack.Add(c);

        return newwep;
    } 

    [Command]
    protected void CmdInteract()
    {
        Ray r = new Ray(Camera.transform.position, Camera.transform.forward);
        RaycastHit hit;

        if(Physics.Raycast(r,out hit,5f))
        {
            /*
            if(hit.collider.GetComponent<Interactable>() != null){
                hit.collider.GetComponent<Interactable>().onInteract();
            }
            */
        }
    }

    private void Recoil()
    {
        Gun g = CurrentGun;
        float x = Random.Range((g.RecoilX.x / 10), g.RecoilX.y / 10);
        float y = Random.Range((g.RecoilY.x / 10), g.RecoilY.y / 10);

        if (isCrouching)
        {
            x /= 2;
            y /= 1.5f;
        }

        Vector2 mouse = im.getMouse();
        float mouseX = mouse.x * (MouseSens * 2);
        float mouseY = mouse.y * (MouseSens * 2);

        xRot -= Mathf.Lerp(mouseY, mouseY + y, 12);
        xRot = Mathf.Clamp(xRot, -75, 75);

        //mouseX = Mathf.Lerp(mouseX,mouseX + x, 0.8f * Time.deltaTime);

        transform.Rotate(Vector3.up * Mathf.Lerp(mouseX,mouseX + x,12));
        Camera.transform.localEulerAngles = Vector3.right * xRot;
    }

    protected void UpdateButtons()
    {
        if (im.getSprint() && !isDowned)
        {
            isRunning = true;

            if (justSlid == false)
            {
                isSliding = false;
            }
        }
        else if (im.getSprint() == false && !isDowned)
        {
            isRunning = false;
            if (justSlid) justSlid = false;
        }

        if(CurrentGun != null && gunTimer < 0 && !isDowned && !firing)
        {
            if (CurrentGun.FireType == Gun.FireMode.Auto && im.getFireHold())
            {
                StartCoroutine(ShootPrimary());
                gunTimer = (60 / CurrentGun.RPM);
            }
            else if (CurrentGun.FireType == Gun.FireMode.Semi && im.getFirePress())
            {
                StartCoroutine(ShootPrimary());
                gunTimer = (60 / CurrentGun.RPM);
            }
        }
        else
        {
            //Debug.Log(CurrentGun + " IS NULL!");
        }

        /*
        // CONVERTE TO NEW INPUTMANAGER
        if (Input.GetKeyDown(KeyCode.F) && !isDowned)
        {
            CmdToggleUtility();
        }
        */

        //if (Input.GetKeyDown(KeyCode.Alpha1) && !isDowned && Primary != null) OnPrimaryPressed();


        /*
        if (Input.GetMouseButtonDown(2))
        {
            Debug.Log("Pining!");
            RaycastHit hit;
            if(Physics.Raycast(Camera.position,Camera.transform.TransformDirection(Vector3.forward),out hit, Mathf.Infinity))
            {
                PingSystem.AddPing(hit.point, PingObj, Camera.gameObject);
                Debug.DrawRay(Camera.position, Camera.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            }
        }
        */

        gunTimer -= Time.deltaTime;
        //Debug.Log(gunTimer);
    }

    protected virtual void OnPrimaryPressed()
    {
        if (CurrentGun != null && CurrentGun.id == Primary.id)
        {
            Debug.Log("De-Equiping Primary");
            DeEquip();
        }
        else
        {
            Debug.Log("Equiping Primary");
            EquipGun(Primary.id);
        }
    }

    protected virtual void EquipGun(int id)
    {
        CmdEquipGun(id);
    }

    protected virtual void DeEquip()
    {
        CmdDeEquip();
    }

    //Return Values: CanWallBang? / 
    [Server]
    bool CanWallPen(Gun.BulletPenatration p,Ray ray,RaycastHit hit,Vector3? endPoint,out Vector3? penPoint,out Vector3? impactPoint)
    {
        float PennAmount = 0;

        if(p == Gun.BulletPenatration.High)
        {
            PennAmount = 2.5f;
        }
        else if(p == Gun.BulletPenatration.Medium)
        {
            PennAmount = 1.5f;
        }
        else if(p == Gun.BulletPenatration.Low)
        {
            PennAmount = .5f;
        }

        impactPoint = hit.point;
        Ray penRay = new Ray(hit.point + ray.direction * PennAmount, -ray.direction);
        RaycastHit penHit;
        if (hit.collider.Raycast(penRay, out penHit, PennAmount))
        {
            penPoint = penHit.point;
            //Call Wall Pen Again
            //endPoint = Camera.transform.position + Camera.transform.TransformDirection(Vector3.forward) * 1000;
            return true;
        }
        else
        {
            //endPoint = impactPoint;
            penPoint = impactPoint;
            return false;
        }
    }

    [Server]
    bool CanWallPenPA(float pa, Ray ray, RaycastHit hit, Vector3? endPoint, out Vector3? penPoint, out Vector3? impactPoint)
    {
        float PennAmount = pa;

        impactPoint = hit.point;
        Ray penRay = new Ray(hit.point + ray.direction * PennAmount, -ray.direction);
        RaycastHit penHit;
        if (hit.collider.Raycast(penRay, out penHit, PennAmount))
        {
            penPoint = penHit.point;
            //Call Wall Pen Again
            //endPoint = Camera.transform.position + Camera.transform.TransformDirection(Vector3.forward) * 1000;
            return true;
        }
        else
        {
            //endPoint = impactPoint;
            penPoint = impactPoint;
            return false;
        }
    }

    void DebugOnWBFire(Vector3 startingpos, Vector3? impactPoint, Vector3? penPoint, Vector3? endPoint)
    {
        float t = 10;

        if (penPoint != null && impactPoint != null)
        {
            Debug.DrawLine(startingpos, impactPoint.Value, Color.yellow, t);
            Debug.DrawLine(impactPoint.Value, penPoint.Value, Color.red, t);
            Debug.DrawLine(penPoint.Value, endPoint.Value, Color.yellow, t);
        }
        else
        {
            Debug.DrawLine(startingpos, endPoint.Value, Color.green, t);
        }
    }

    void DebugOnFire(Vector3 startingpos,Vector3? impactPoint, Vector3? penPoint, Vector3? endPoint)
    {
        float t = 10;

        if (penPoint != null && impactPoint != null)
        {
            Debug.DrawLine(startingpos, impactPoint.Value, Color.green, t);
            Debug.DrawLine(impactPoint.Value, penPoint.Value, Color.red, t);
            Debug.DrawLine(penPoint.Value, endPoint.Value, Color.yellow, t);
        }
        else
        {
            Debug.DrawLine(startingpos, endPoint.Value, Color.green, t);
        }
    }

    [ClientRpc]
    void RpcBulletTrail(Vector3 start,Vector3 end)
    {
        GameObject bt = Instantiate(BulletTracer, start, Quaternion.identity);
        bt.GetComponent<TrailRenderer>().AddPosition(end);
        //Destroy(bt, .25f);
    }


    bool firing = false;
    [Client]
    IEnumerator ShootPrimary()
    {
        if (CurrentGun != null && gunTimer <= 0 && p_Mag > 0 && firing == false)
        {
            firing = true;

            for (int i = 0; i < CurrentGun.ShotsPerFire; i++)
            {
                if (p_Mag <= 0)
                {
                    firing = false;
                    break;
                }
                Vector3 firedir = Camera.transform.position + Camera.transform.forward * 1000;

                //adding inaccuracy UP and Right (POSITIVE AND NEGATIVE VALUES)
                firedir += Random.Range(-getBloom(), getBloom()) * Camera.transform.up;
                firedir += Random.Range(-getBloom(), getBloom()) * Camera.transform.right;

                firedir = firedir - Camera.transform.position;

                //firing cmd
                //Debug.Log("FIRING TO CLIENT");
                // + Camera.transform.forward * .5f
                CmdShowFire(Camera.transform.position, firedir);
                Recoil();
                p_Mag--;
                yield return new WaitForSeconds(CurrentGun.DelayPerShot / 100);
            }

            gunTimer = (60 / CurrentGun.RPM);
            firing = false;
        }
    }

    [Command]
    void CmdShowFire(Vector3 cpos,Vector3 cfwrd)
    {
        Vector3? endPoint = null;
        Vector3? penPoint = null;
        Vector3? impactPoint = null;
        Vector3 firstwallhit;

        Ray ray = new Ray(cpos, cfwrd);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            endPoint = hit.point;
            penPoint = null;
            impactPoint = null;
            firstwallhit = endPoint.Value;

            //Debug.Log(hit.collider.transform.root.name + "    " + hit.collider.transform.root.GetComponent<Player>());
            Player p = hit.collider.transform.root.GetComponent<Player>();

            if (p != null && !p.playerName.Equals(playerName))
            {
                //Debug.Log(p.playerName + " / " + this.playerName);
                p.TakeDamage(cc.gameObject.name, CurrentGun.BaseDamage);
            }
            else
            {
                //WallBang
                if (CanWallPen(CurrentGun.BulletPen, ray, hit, endPoint, out penPoint, out impactPoint))
                {
                    //Debug.Log("Wallbanging");

                    float PennAmount = 0f;

                    if (CurrentGun.BulletPen == Gun.BulletPenatration.High)
                    {
                        PennAmount = 2.5f;
                    }
                    else if (CurrentGun.BulletPen == Gun.BulletPenatration.Medium)
                    {
                        PennAmount = 1.5f;
                    }
                    else if (CurrentGun.BulletPen == Gun.BulletPenatration.Low)
                    {
                        PennAmount = .5f;
                    }

                    PennAmount -= .5f;

                    Ray aftershot = new Ray(penPoint.Value, ray.direction);
                    RaycastHit afterhit;
                    if (Physics.Raycast(aftershot, out afterhit))
                    {
                        p = afterhit.collider.transform.root.GetComponent<Player>();
                        //Debug.Log("Object Hit: " + afterhit.collider.gameObject.name + " AND HAS PLAYER SCRIPT: " + p != null);

                        if (p != null && !p.playerName.Equals(playerName))
                        {
                            //Debug.Log("HIT PLAYER!");
                            float Bulletwallt = Vector3.Distance(impactPoint.Value, penPoint.Value);
                            //Debug.Log("Bullet Traveled: " + Bulletwallt);
                            float fall = (CurrentGun.FallOffDamage * (Bulletwallt * 10));
                            int dmg = Mathf.RoundToInt(CurrentGun.BaseDamage / (fall / CurrentGun.FallOffDamage));
                            dmg = Mathf.Clamp(dmg, CurrentGun.BaseDamage * ((CurrentGun.FallOffDamage * 2) / 100), CurrentGun.BaseDamage);
                            //Debug.Log("Weapon Damage Fall Off %" + fall / CurrentGun.FallOffDamage + ". With Damage Being: " + dmg);
                            p.TakeDamage(playerName, dmg);
                            endPoint = afterhit.point;
                        }
                        else
                        {
                            endPoint = afterhit.point;
                        }
                    }
                    else
                    {
                        //no where to hit
                        endPoint = ray.direction * 100;
                    }
                }


                DebugOnWBFire(Camera.position, impactPoint, penPoint, endPoint);
            }
        }
        else
        {
            endPoint = Camera.transform.position + Camera.transform.forward * 250;
            penPoint = null;
            impactPoint = null;
            firstwallhit = endPoint.Value;

            DebugOnFire(Camera.position, impactPoint, penPoint, endPoint);
        }

        RpcBulletTrail(Camera.position, endPoint.Value);
    }


    float ForwardVelocity;
    bool justSlid = false;
    protected void Movement()
    {
        isCrouching = (im.getCrouch());

        if (!isGrounded)
        {
            isCrouching = false;
        }

        if (ForwardVelocity > 0 && !isGrounded)
        {
            ForwardVelocity -= Time.deltaTime;
        }
        else if (isGrounded || ForwardVelocity <= 0)
        {
            ForwardVelocity = 0;
        }


        Vector2 inputDir = im.getMovement();

        if(canMove == false)
        {
            inputDir = new Vector2(0, 0);
            isCrouching = false;
        }

        if (isDowned)
        {
            currentSpeed = DownedSpeed;
        }
        else if (isRunning && inputDir.y >= 0.05)
        {
            currentSpeed = RunSpeed;
        }
        else if(isCrouching)
        {
            currentSpeed = CrouchSpeed;
        }
        else
        {
            currentSpeed = WalkSpeed;
        }

        /*
        if (isCrouching && isRunning && isSliding == false)
        {
            slideDir = (transform.forward * inputDir.y + transform.right * inputDir.x);
            Slide();
            justSlid = true;
            isRunning = false;
        }
        */

        if(!isDowned) CmdShowCrouch(isSliding || isCrouching);

        if (isSliding)
        {
            // Stoping Player Movment
            UpdateSlide();
            inputDir = new Vector2(0, 0);

            /*
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                isSliding = false;
                Debug.Log("Slide Cancel");
            }
            */
        }

        // Math To Get Jumping While Sliding Giving A Small Boost In The Air
        float dashairspeed = ForwardVelocity * WalkSpeed;
        //Mathf.Clamp((1 - (timePassed / slideTime)),0.4f,1)) Longer You Slide Farther You Jump
        Vector3 vel = (transform.forward * inputDir.y + transform.forward * (dashairspeed * .30f) + transform.right * inputDir.x) * (currentSpeed / 100) + Vector3.up * airSpeed;
        cc.Move(vel);
    }

    [Command]
    protected void CmdShowCrouch(bool c)
    {
        RpcUpdateCrouch(c);
        RpcTUpdateCrouch(c);
    }

    [TargetRpc]
    protected void RpcTUpdateCrouch(bool c)
    {
        //if (Viewmodel == null) return;
        if (c)
        {
            Camera.transform.localPosition = new Vector3(0, .25f, 0);
        }
        else
        {
            Camera.transform.localPosition = new Vector3(0, .5f, 0);
        }
    }

    [ClientRpc]
    protected void RpcUpdateCrouch(bool c)
    {
        if (c)
        {
            PlayerBody.transform.localScale = new Vector3(1,.75f,1);
            cc.height = 1;
        }
        else
        {
            PlayerBody.transform.localScale = new Vector3(1,1,1);
            cc.height = 2;
        }
    }

    protected void UpdateSlide()
    {
        if(timePassed <= 0)
        {
            isSliding = false;
        }

        Vector3 vel = slideDir * ((slideSpeed * (timePassed / slideTime))/100);
        //Debug.Log(vel);
        cc.Move(vel);

        timePassed -= Time.deltaTime;
    }

    protected void Vualt()
    {

    }

    protected void Slide()
    {
        isSliding = true;
        timePassed = slideTime;
    }

    float xRot = 0.0f;
    protected void CameraMovement()
    {
        Vector2 m = im.getMouse();
        float mouseX = m.x * (MouseSens * 2);
        float mouseY = m.y * (MouseSens * 2);

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -75, 75);

        //Debug.Log(mouseX);

        transform.Rotate(Vector3.up * mouseX);
        Camera.transform.localEulerAngles = Vector3.right * xRot;
    }

    protected void UpdateGravity()
    {
        isGrounded = cc.isGrounded;
        if (isGrounded)
        {
            airSpeed = -.5f;
        }

        airSpeed -= Gravity * Time.deltaTime;
    }

    [Server]
    public void Revive()
    {
        isDowned = false;
        DownedHealth = 100;
        Health = 50;
        RpcOnEquip(CurrentGun.id);
    }

    [TargetRpc]
    protected void RpcOnKnock()
    {
        if (Viewmodel != null)
        {
            Destroy(Viewmodel);
        }

        onDowned();
    }

    [Server]
    public virtual void TakeDamage(string name,int dmg)
    {
        lasttookdmgfrom = name;

        if (isDowned)
        {
            //Debug.Log("Alr Downed");
            DownedHealth -= dmg * .75f;
        }
        else
        {
            float max = Shield + Health;
            max -= dmg;

            Shield = Mathf.Max(0, (max - 100));

            max -= Shield;
            Health = Mathf.Max(0, max);

            if (Health <= 0)
            {
                //Debug.Log("Is Downed!");
                isDowned = true;

                RpcUpdateCrouch(true);
                RpcTUpdateCrouch(true);

                Health = 0;

                // Remove Gun
                if (CurrentGun != null)
                {
                    RpcOnKnock();

                    RpcToggleNV(false);
                    RpcUpdateFlashlight(false);
                    ToggledUtil = false;

                }
            }

           //Debug.Log("Health: " + Health + " | Shield: " + Shield);
        }
    }

    [Client]
    protected void UpdateHealth(float oldhealth, float nhealth)
    {
        DownedRing.gameObject.SetActive(isDowned);
        //Debug.Log("Health: " + Shield + " | Shield: " + Health);

        HealthBar.fillAmount = Health / 100;
        HealthText.text = Mathf.RoundToInt(Health).ToString();
    }

    [Client]
    protected void UpdateDownedHealth()
    {
        if (isDowned)
        {
            DownedRing.gameObject.SetActive(isDowned);
            DownedRing.fillAmount = DownedHealth / 100;
        }
    }

    [Client]
    protected void UpdateShield(float oldshield, float newshield)
    {
        //Debug.Log("Health: " + Shield + " | Shield: " + Health);
        ShieldBar.fillAmount = Shield/50;
        ShieldText.text = Mathf.RoundToInt(Shield).ToString();
    }

    [Client]
    protected virtual void onDowned() { }
}