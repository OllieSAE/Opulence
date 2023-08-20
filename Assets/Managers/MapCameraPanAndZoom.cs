using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MapCameraPanAndZoom : MonoBehaviour
{
    [SerializeField] private float defaultSpeed = 0.5f;
    [SerializeField] private float fastSpeed = 1f;
    private float panSpeed;
    [SerializeField] private float zoomSpeed = 1f;
    private CinemachineInputProvider inputProvider;
    private CinemachineVirtualCamera mapCamera;
    private Transform mapCameraTransform;
    [SerializeField] private Transform followTransform;
    [SerializeField] private float zoomInMax;
    [SerializeField] private float zoomOutMax;

    private void Awake()
    {
        inputProvider = GetComponent<CinemachineInputProvider>();
        mapCamera = GetComponent<CinemachineVirtualCamera>();
        mapCameraTransform = mapCamera.VirtualCameraGameObject.transform;
    }
    

    private void Start()
    {
        GameManager.Instance.mapOpenedEvent += MapOpened;
        GameManager.Instance.mapClosedEvent += MapClosed;
    }

    private void OnDisable()
    {
        GameManager.Instance.mapOpenedEvent -= MapOpened;
        GameManager.Instance.mapClosedEvent -= MapClosed;
    }

    private void MapOpened()
    {
        followTransform = mapCamera.m_Follow;
        mapCamera.m_Follow = null;
    }

    private void MapClosed()
    {
        //snap back isn't working, maybe set camera transform to followTransform + Z offset
        //mapCamera.m_Follow = followTransform;
        mapCameraTransform.position = followTransform.position + new Vector3(0, 0, -20);
    }

    private void Update()
    {
        float x = inputProvider.GetAxisValue(0);
        float y = inputProvider.GetAxisValue(1);
        float z = inputProvider.GetAxisValue(2);

        if (x != 0 || y != 0) PanScreen(x, y);
        if (z != 0) ZoomScreen(z);
    }

    public void ZoomScreen(float amount)
    {
        float ortho = mapCamera.m_Lens.OrthographicSize;
        float target = Mathf.Clamp(ortho + amount, zoomInMax, zoomOutMax);
        mapCamera.m_Lens.OrthographicSize = Mathf.Lerp(ortho, target, zoomSpeed);
    }

    public Vector2 PanDirection(float x, float y)
    {
        Vector2 dir = Vector2.zero;
        if (y >= Screen.height * 0.8f)
        {
            if (y >= Screen.height * 0.95f)
            {
                panSpeed = fastSpeed;
                dir.y += 1;
            }
            else
            {
                panSpeed = defaultSpeed;
                dir.y += 1;
            }
            
        }
        else if (y <= Screen.height * 0.2f)
        {
            if (y <= Screen.height * 0.05f)
            {
                panSpeed = fastSpeed;
                dir.y -= 1;
            }
            else
            {
                panSpeed = defaultSpeed;
                dir.y -= 1;
            }
        }

        if (x >= Screen.width * 0.8f)
        {
            if (x >= Screen.width * 0.95f)
            {
                panSpeed = fastSpeed;
                dir.x += 1;
            }
            else
            {
                panSpeed = defaultSpeed;
                dir.x += 1;
            }
            
        }
        else if (x <= Screen.width * 0.2f)
        {
            if (x <= Screen.width * 0.05f)
            {
                panSpeed = fastSpeed;
                dir.x -= 1;
            }
            else
            {
                panSpeed = defaultSpeed;
                dir.x -= 1;
            }
        }
        return dir;
    }

    public void PanScreen(float x, float y)
    {
        Vector2 dir = PanDirection(x, y);
        Vector3 pos = mapCameraTransform.position;
        pos.x = Mathf.Clamp(mapCameraTransform.position.x, 250, 350);
        pos.y = Mathf.Clamp(mapCameraTransform.position.y, -50, 50);
        mapCameraTransform.position = pos;
        mapCameraTransform.position = Vector3.Lerp(
            mapCameraTransform.position,
            mapCameraTransform.position + (Vector3)dir,
            panSpeed);
    }
}
