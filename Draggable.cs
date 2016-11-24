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
        Finish, Start
    }

    public MovementType movement;
    public int startPosition;
    public int finishPosition;
    public UnityEvent onUp;
    public UnityEvent onDown;

    public Position transformPosition;

    private RectTransform objectToDrag;
    private float lastDelta;
    private bool isDragging;
    private Vector2 newAnchoredPosition;

    public RectTransform ObjectToDrag
    {
        get
        {
            if (this.objectToDrag == null)
            {
                this.objectToDrag = this.GetComponent<RectTransform>();
            }

            return this.objectToDrag;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        this.lastDelta = 0;
        this.isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.isDragging = true;
        this.newAnchoredPosition = eventData.delta;
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
        StartCoroutine(FinishToTheEnd());
    }

    public void GoUp(float power = 0.5f)
    {
        if (this.isDragging)
        {
            return;
        }

        this.lastDelta = power;
        StartCoroutine(FinishToTheEnd());
    }

    public void GoDown(float power = 0.5f)
    {
        if (this.isDragging)
        {
            return;
        }

        this.lastDelta = -power;

        if (this.isActiveAndEnabled)
        {
            StartCoroutine(FinishToTheEnd());
        }
    }

    public void Switch()
    {
        //Prevent double call when release pointer
        if (this.isDragging)
        {
            return;
        }

        switch (this.transformPosition)
        {
            case Position.Finish:
                GoUp();
                break;
            case Position.Start:
                GoDown();
                break;
        }
    }

    private IEnumerator FinishToTheEnd()
    {
        if (this.isDragging)
        {
            yield break;
        }

        float position = 0;

        do
        {
            float delta = Mathf.Sign(this.lastDelta) >= 0 ? this.lastDelta += 1f : this.lastDelta -= 1f;
            switch (this.movement)
            {
                case MovementType.Horizontal:
                    position = this.ObjectToDrag.anchoredPosition.x;
                    this.newAnchoredPosition = new Vector2(delta, 0);
                    Translate();
                    break;
                case MovementType.Vertical:
                    position = this.ObjectToDrag.anchoredPosition.y;
                    this.newAnchoredPosition = new Vector2(0, delta);
                    Translate();
                    break;
            }
            yield return new WaitForEndOfFrame();
        } while (this.startPosition < position && position < this.finishPosition);

        if (this.startPosition <= position)
        {
            this.transformPosition = Position.Finish;
            this.onUp.Invoke();
        }

        if (position >= this.finishPosition)
        {
            this.transformPosition = Position.Start;
            this.onDown.Invoke();
        }

        this.lastDelta = 0;
    }

    private void Translate()
    {
        switch (this.movement)
        {
            case MovementType.Horizontal:
                this.newAnchoredPosition.x = ClampPosition(this.ObjectToDrag.anchoredPosition.x + this.newAnchoredPosition.x);
                this.newAnchoredPosition.y = this.ObjectToDrag.anchoredPosition.y;
                break;
            case MovementType.Vertical:
                this.newAnchoredPosition.x = this.ObjectToDrag.anchoredPosition.x;
                this.newAnchoredPosition.y = ClampPosition(this.ObjectToDrag.anchoredPosition.y + this.newAnchoredPosition.y);
                break;
        }

        this.ObjectToDrag.anchoredPosition = this.newAnchoredPosition;
    }

    private float ClampPosition(float currentPosition)
    {
        return Mathf.Clamp(currentPosition, this.startPosition, this.finishPosition);
    }

    [ContextMenu("Move to start position")]
    public void MoveToStart()
    {
        switch (this.movement)
        {
            case MovementType.Horizontal:
                this.newAnchoredPosition.x = this.startPosition;
                this.newAnchoredPosition.y = this.ObjectToDrag.anchoredPosition.y;
                break;
            case MovementType.Vertical:
                this.newAnchoredPosition.x = this.ObjectToDrag.anchoredPosition.x;
                this.newAnchoredPosition.y = this.startPosition;
                break;
        }

        this.ObjectToDrag.anchoredPosition = this.newAnchoredPosition;
    }

    [ContextMenu("Move to finish position")]
    public void MoveToFinish()
    {
        switch (this.movement)
        {
            case MovementType.Horizontal:
                this.newAnchoredPosition.x = this.finishPosition;
                this.newAnchoredPosition.y = this.ObjectToDrag.anchoredPosition.y;
                break;
            case MovementType.Vertical:
                this.newAnchoredPosition.x = this.ObjectToDrag.anchoredPosition.x;
                this.newAnchoredPosition.y = this.startPosition;
                break;
        }

        this.ObjectToDrag.anchoredPosition = this.newAnchoredPosition;
    }
}
