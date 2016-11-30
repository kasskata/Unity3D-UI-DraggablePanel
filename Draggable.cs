using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum MovementType
    {
        Horizontal, Vertical
    }

    public enum State
    {
        Start, End
    }

    private readonly Vector2 UpRight = Vector2.up + Vector2.right;
    private readonly Vector2 DownLeft = Vector2.down + Vector2.left;

    [Space(5)]
    public MovementType direction;
    public State state;
    public float inertiaFactor;

    [Space(5)]
    [HideInInspector]
    public float startPosition;

    [HideInInspector]
    public float endPosition;

    [Space(5)]
    public UnityEvent onStart;
    public UnityEvent onEnd;

    //Prevent OnClick event when drag.
    private bool isDragging;
    private RectTransform draggableObjectCache;
    private Vector2 lastDelta;
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
        this.lastDelta = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.newDragPosition = eventData.delta;
        Translate();

        this.lastDelta = eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.isDragging = false;
        StartCoroutine(InertiaComplete());
    }

    public void GoForward()
    {
        if (this.isDragging)
        {
            return;
        }

        if (this.state == State.Start)
        {
            this.lastDelta = this.UpRight;
            StartCoroutine(InertiaComplete());
        }
    }

    public void GoReverse()
    {
        if (this.isDragging)
        {
            return;
        }

        if (this.state == State.End)
        {
            this.lastDelta = this.DownLeft;
            StartCoroutine(InertiaComplete());
        }
    }


    public void Switch()
    {
        if (this.isDragging)
        {
            return;
        }

        switch (this.state)
        {
            case State.Start:
                GoForward();
                break;
            case State.End:
                GoReverse();
                break;
        }
    }

    private IEnumerator InertiaComplete()
    {
        float position;

        Vector2 signXY = new Vector2(Mathf.Sign(this.lastDelta.x), Mathf.Sign(this.lastDelta.y));

        do
        {
            this.lastDelta.x = signXY.x >= 0 ? this.lastDelta.x += this.inertiaFactor : this.lastDelta.x -= this.inertiaFactor;
            this.lastDelta.y = signXY.y >= 0 ? this.lastDelta.y += this.inertiaFactor : this.lastDelta.y -= this.inertiaFactor;

            this.newDragPosition = this.lastDelta;
            Translate();

            yield return new WaitForEndOfFrame();
            position = this.direction == MovementType.Horizontal
                ? this.draggableObjectCache.anchoredPosition.x
                : this.draggableObjectCache.anchoredPosition.y;
        } while (this.startPosition < position && position < this.endPosition);

        if (this.startPosition <= position && this.state != State.Start)
        {
            MoveToStart();
            this.state = State.Start;
            this.onStart.Invoke();
        }

        if (position >= this.endPosition && this.state != State.End)
        {
            MoveToEnd();
            this.state = State.End;
            this.onEnd.Invoke();
        }

        Debug.Log(this.state);
        this.lastDelta = Vector2.zero;
    }

    private void Translate()
    {
        switch (this.direction)
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
        return Mathf.Clamp(currentPosition, this.startPosition, this.endPosition);
    }

    [ContextMenu("Move to Bottom Or Left position")]
    private void MoveToStart()
    {
        switch (this.direction)
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

    [ContextMenu("Move to Top Or Right position")]
    private void MoveToEnd()
    {
        switch (this.direction)
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

#if UNITY_EDITOR
[CustomEditor(typeof(Draggable))]
public class DraggableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawPropertiesExcluding(this.serializedObject, "m_Script", "onStart", "onEnd");

        Draggable draggableScript = (Draggable)this.target;
        switch (draggableScript.direction)
        {
            case Draggable.MovementType.Horizontal:
                GUILayout.BeginHorizontal("Borders", GUILayout.MaxHeight(40f));
                EditorGUILayout.LabelField("Left", GUILayout.MaxWidth(50f));
                draggableScript.startPosition = EditorGUILayout.FloatField(draggableScript.startPosition);
                EditorGUILayout.LabelField("Right", GUILayout.MaxWidth(60f));
                draggableScript.endPosition = EditorGUILayout.FloatField(draggableScript.endPosition);
                GUILayout.EndHorizontal();
                break;
            case Draggable.MovementType.Vertical:
                GUILayout.BeginHorizontal("Borders", GUILayout.MaxHeight(40f));
                EditorGUILayout.LabelField("Top", GUILayout.MaxWidth(50f));
                draggableScript.endPosition = EditorGUILayout.FloatField(draggableScript.endPosition);
                EditorGUILayout.LabelField("Bottom", GUILayout.MaxWidth(60f));
                draggableScript.startPosition = EditorGUILayout.FloatField(draggableScript.startPosition);
                GUILayout.EndHorizontal();
                break;
        }

        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("onStart"), true);
        EditorGUILayout.PropertyField(this.serializedObject.FindProperty("onEnd"), true);

        this.serializedObject.ApplyModifiedProperties();
    }
}
#endif