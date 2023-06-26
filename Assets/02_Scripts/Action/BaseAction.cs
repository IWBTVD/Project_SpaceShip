using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class BaseAction : MonoBehaviour
{
    public static event EventHandler OnAnyActionStarted;
    public static event EventHandler OnAnyActionCompleted;

    protected Unit unit;
    protected bool isActive;
    protected Action onActionComplete;

    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
    }

    //버튼에 들어갈 글자 리턴
    public abstract string GetActionName();

    //액션 수행 시 호출되는 함수
    public abstract void TakeAction(Vector3 worldPosition, Action onActionComplete);

    /// <summary>
    /// 액션이 유효한 위치에 놓였는지
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    public virtual bool IsValidActionPosition(Vector3 worldPosition)
    {
        return false;
    }

    public virtual int GetActionPointCost()
    {
        //default action cost
        return 1;
    }

    protected void ActionStart(Action onActionComplete)
    {
        isActive = true;
        this.onActionComplete = onActionComplete;

        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }

    protected void ActionComplete()
    {
        isActive = false;
        onActionComplete();

        OnAnyActionCompleted?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetUnit()
    {
        return unit;
    }
}
