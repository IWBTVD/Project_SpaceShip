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

    //��ư�� �� ���� ����
    public abstract string GetActionName();

    //�׼� ���� �� ȣ��Ǵ� �Լ�
    public abstract void TakeAction(Vector3 worldPosition, Action onActionComplete);

    /// <summary>
    /// �׼��� ��ȿ�� ��ġ�� ��������
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
