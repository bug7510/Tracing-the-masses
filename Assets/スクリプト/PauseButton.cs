using DG.Tweening;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] Sprite willPlayImg;
    [SerializeField] Sprite willPauseImg;
    Outline outline;
    Image imageComponent;
    Tween rotate;
    Tween backgroundRotate;
    [SerializeField] Transform menuBackgroundTransform;
    public void Rotate(int angle)
    {
        if (rotate != null && rotate.IsPlaying()) rotate.Complete();
        rotate = transform.DORotate(new(0, 0, angle + transform.rotation.eulerAngles.z), 2f)
                            .SetEase(Ease.InOutBack)
                            .SetDependency(() => GameTime.isGaming);
        if (backgroundRotate != null && backgroundRotate.IsPlaying()) backgroundRotate.Complete();
        backgroundRotate = menuBackgroundTransform.DORotate(new(0, 0, angle + transform.rotation.eulerAngles.z), 2f)
                                        .SetEase(Ease.InOutBack)
                                        .SetDependency(() => GameTime.isGaming);
    }
    [SerializeField] float phaseDuration;
    public void PhaseColor()
    {
        if (GameSceneManager.phase == Phase.tracing)
        {
            outline.DOColor(Color.white, phaseDuration).SetDependency(() => GameTime.isGaming);
            if (imageColorChange == null || !onPoint) imageColorChange = imageComponent.DOColor(Color.black, phaseDuration).SetDependency(() => GameTime.isGaming);
        }
        else
        {
            outline.DOColor(Color.black, phaseDuration).SetDependency(() => GameTime.isGaming);
            if (imageColorChange == null || !onPoint) imageColorChange = imageComponent.DOColor(Color.white, phaseDuration).SetDependency(() => GameTime.isGaming);
        }
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) Pause(new());
    }
    [SerializeField] Image pauseCover;
    [SerializeField] GameObject pauseMenu;
    void Pause(InputAction.CallbackContext context)
    {
        pauseCover.enabled = GameTime.isGaming;
        pauseMenu.SetActive(GameTime.isGaming);
        GameTime.isGaming = !GameTime.isGaming;
        if (GameTime.isGaming) imageComponent.sprite = willPauseImg;
        else imageComponent.sprite = willPlayImg;
    }
    [SerializeField] Color colorOnPoint;
    [SerializeField] float pointerDuration;
    bool onPoint;
    Tweener imageColorChange;
    public void OnPointerEnter(PointerEventData eventData)
    {
        imageColorChange?.Kill(true);
        onPoint = true;
        imageColorChange = imageComponent.DOColor(colorOnPoint, pointerDuration);
    }
    [SerializeField] InputActionReference _pauseAction;
    InputAction pauseAction;
    public void OnPointerExit(PointerEventData eventData)
    {
        imageColorChange?.Kill();
        onPoint = false;
        if (GameSceneManager.phase == Phase.tracing) imageColorChange = imageComponent.DOColor(Color.black, phaseDuration);
        else imageColorChange = imageComponent.DOColor(Color.white, phaseDuration);
    }
    #region Method for Button
    [SerializeField] SceneReference firstScene;
    public void BackTo1stScene()
    {
        firstScene.LoadScene();
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion
    void Awake()
    {
        outline = GetComponent<Outline>();
        imageComponent = GetComponent<Image>();
        pauseAction = _pauseAction.action;
    }
    void Start()
    {
        pauseCover.enabled = false;
        pauseMenu.SetActive(false);
    }
    void OnEnable()
    {
        pauseAction.Enable();
        pauseAction.performed += Pause;
    }
    void OnDisable()
    {
        pauseAction.performed -= Pause;
        pauseAction.Disable();
    }
}
