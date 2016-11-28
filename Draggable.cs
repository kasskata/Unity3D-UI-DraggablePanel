using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum MovementType
    {
        Horizontal, Vertical
    }

    public enum Position
    {
        Start, End
    }

    [Header("Translate type")]
    public MovementType movement;
    public float inertiaFactor;

    [Space(5)]
    [Header("Borders")]
    public float startPosition;
    public float endPosition;

    [Space(5)]
    [HideInInspector]
    public Position positionStay;

    [Space(5)]
    public UnityEvent onReverseFinish;
    public UnityEvent onForwardFinish;

    private RectTransform draggableObjectCache;
    private float lastDelta;
    private bool isDragging;
    private Vector2 newDragPosition;

    public RectTransform DraggableObjectCache
    {
        get
        {
            if (this.draggableObjectCache == null)
            {
                this.draggableObjectCache = this.GetComponent<RectTransform>();
            }

            return this.draggableObjectCache;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        this.isDragging = true;
        this.lastDelta = 0;
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.newDragPosition = eventData.delta;
        Translate();

        switch (this.movement)
        {
            case MovementType.Horizontal:
                this.lastDelta = eventData.delta.x;
                break;
            case MovementType.Vertical:
                this.lastDelta = eventData.delta.y;
                break;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.isDragging = false;
        StartCoroutine(InertiaComplete());
    }

    public void GoReverse(float power = 0.5f)
    {
        if (this.isDragging)
        {
            return;
        }

        this.inertiaFactor = power;
        StartCoroutine(InertiaComplete());
    }

    public void GoForward(float power = 0.5f)
    {
        if (this.isDragging)
        {
            return;
        }

        this.inertiaFactor = -power;
        StartCoroutine(InertiaComplete());
    }

    public void Switch()
    {
        //Prevent double call when release pointer
        if (this.isDragging)
        {
            return;
        }

        switch (this.positionStay)
        {
            case Position.End:
                GoReverse();
                break;
            case Position.Start:
                GoForward();
                break;
        }
    }

    private IEnumerator InertiaComplete()
    {
        if (this.isDragging)
        {
            yield break;
        }

        float position = 0;

        do
        {
            float delta = Mathf.Sign(this.lastDelta) >= 0 ?
                this.lastDelta += this.inertiaFactor :
                this.lastDelta -= this.inertiaFactor;


            switch (this.movement)
            {
                case MovementType.Horizontal:
                    position = this.DraggableObjectCache.anchoredPosition.x;
                    this.newDragPosition = new Vector2(delta, 0);
                    break;
                case MovementType.Vertical:
                    position = this.DraggableObjectCache.anchoredPosition.y;
                    this.newDragPosition = new Vector2(0, delta);
                    break;
            }

            Translate();
            yield return new WaitForEndOfFrame();
        } while (this.startPosition < position && position < this.endPosition);

        if (this.startPosition <= position)
        {
            this.positionStay = Position.Start;
            this.onReverseFinish.Invoke();
        }

        if (position >= this.endPosition)
        {
            this.positionStay = Position.End;
            this.onForwardFinish.Invoke();
        }

        this.lastDelta = 0;
    }

    private void Translate()
    {
        switch (this.movement)
        {
            case MovementType.Horizontal:
                this.newDragPosition.x = ClampPosition(this.DraggableObjectCache.anchoredPosition.x + this.newDragPosition.x);
                this.newDragPosition.y = this.DraggableObjectCache.anchoredPosition.y;
                break;
            case MovementType.Vertical:
                this.newDragPosition.x = this.DraggableObjectCache.anchoredPosition.x;
                this.newDragPosition.y = ClampPosition(this.DraggableObjectCache.anchoredPosition.y + this.newDragPosition.y);
                break;
        }

        this.DraggableObjectCache.anchoredPosition = this.newDragPosition;
    }

    private float ClampPosition(float currentPosition)
    {
        if (this.startPosition < this.endPosition)
        {
            return Mathf.Clamp(currentPosition, this.startPosition, this.endPosition);
        }
        else
        {
            return Mathf.Clamp(currentPosition, this.endPosition, this.startPosition);
        }
    }

    [ContextMenu("Move to start position")]
    private void MoveToStart()
    {
        switch (this.movement)
        {
            case MovementType.Horizontal:
                this.newDragPosition.x = this.startPosition;
                this.newDragPosition.y = this.DraggableObjectCache.anchoredPosition.y;
                break;
            case MovementType.Vertical:
                this.newDragPosition.x = this.DraggableObjectCache.anchoredPosition.x;
                this.newDragPosition.y = this.startPosition;
                break;
        }

        this.DraggableObjectCache.anchoredPosition = this.newDragPosition;
    }

    [ContextMenu("Move to finish position")]
    private void MoveToFinish()
    {
        switch (this.movement)
        {
            case MovementType.Horizontal:
                this.newDragPosition.x = this.endPosition;
                this.newDragPosition.y = this.DraggableObjectCache.anchoredPosition.y;
                break;
            case MovementType.Vertical:
                this.newDragPosition.x = this.DraggableObjectCache.anchoredPosition.x;
                this.newDragPosition.y = this.endPosition;
                break;
        }

        this.DraggableObjectCache.anchoredPosition = this.newDragPosition;
    }

}