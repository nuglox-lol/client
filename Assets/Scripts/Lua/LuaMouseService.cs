using MoonSharp.Interpreter;
using UnityEngine;
using System;

[MoonSharpUserData]
public class LuaMouseService
{
    public event Action Button1Down;
    public event Action Button1Up;

    private Camera mainCamera;

    public LuaMouseService()
    {
        mainCamera = Camera.main;
    }

    public DynValue Hit
    {
        get
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return UserData.Create(new InstanceDatamodel(hit.collider.gameObject));
            }
            return DynValue.Nil;
        }
    }

    public InstanceDatamodel Target
    {
        get
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return new InstanceDatamodel(hit.collider.gameObject);
            }
            return null;
        }
    }

    public Vector3 MousePosition
    {
        get
        {
            return Input.mousePosition;
        }
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Button1Down?.Invoke();
        }
        if (Input.GetMouseButtonUp(0))
        {
            Button1Up?.Invoke();
        }
    }
}