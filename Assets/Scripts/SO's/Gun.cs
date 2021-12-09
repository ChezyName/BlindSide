using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Mirror;

[CreateAssetMenu(fileName = "New Gun",menuName = "Weapons/New Gun")]
public class Gun : ScriptableObject
{
    [Header("Identifaction Number")]
    public int id;

    [Header("Basic Info")]
    public string WeaponName;

    public WeaponType Weapontype; // field
    public enum WeaponType { Primary, Secondary }; // nested type

    public Weapon GunType; // field
    public enum Weapon { AssultRifle, Sniper, Shotgun, Pistol, SMG }; // nested type

    public GameObject Viewmodel;
    public Sprite WeaponIcon;

    [Header("Damage")]
    public int BaseDamage;
    public int HeadShotDamage;
    public int Extremities;

    [Header("Bloom / Innacuraccy")]
    [Tooltip("Base Innacruccy")]
    public float BaseBloom;
    public float AirBloom;
    public float RunBloom;
    public float CrouchBloom;

    [Header("Firing")]
    public FireMode FireType; // field
    public enum FireMode { Semi, Auto}; // nested type
    [Tooltip("RPS = 60 / RPM")]
    public int RPM = 600;
    [Tooltip("How Many Bullets Are Fired When Trigger Is Pulled")]
    [Range(1,25)]
    public int ShotsPerFire = 1;
    [Tooltip("Delay Per Shot / 60")]
    public float DelayPerShot = 25f;

    [Header("Bullet Pen")]
    [Tooltip("Shoot Through Objects + Reduced Damage")]
    public BulletPenatration BulletPen; // field
    public enum BulletPenatration {None,Low,Medium,High}; // nested type
    [Range(0, 100)]
    [Tooltip("Persentage Of Damage Falloff Tough Each Wall (%)")]
    public int FallOffDamage;

    [Header("Recoil")]
    [Tooltip("How Much Recoil After Every Shot for X - (MIN,MAX)")]
    public Vector2 RecoilX;
    [Tooltip("How Much Recoil After Every Shot for Y - (MIN,MAX)")]
    public Vector2 RecoilY;
    [Tooltip("How Many Seconds Until Player Can Move Gun Back To ZERO")]
    public int RecoilResetTime;

    [Header("Ammo")]
    public int MagSize;
    public int MaxAmmo;
}