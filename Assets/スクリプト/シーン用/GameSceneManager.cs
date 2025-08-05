using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Eflatun.SceneReference;

public enum Phase
{
    tracing,
    modeling
}
[Flags]
public enum Harass
{
    none = 0,
    bigger = 1,
    dummy = 2,
    fading = 4,
    rotate90 = 8,
    rotate45 = 16,
    rotateInTracing90 = 32,
    rotateInTracing45 = 64
}
public class GameSceneManager : GameTime
{
    public static GameSceneManager instance;
    public static Phase phase = Phase.modeling;
    public static RandomPathGenerator pathGenerator = new();
    static int mapSize = 4;
    public static int MapSize
    {
        set
        {
            if (mapSize == value) return;
            instance.massMap.ResizeMap(value);
            mapSize = value;
        }
        get => mapSize;
    }
    InputAction fourDirectionActions;
    [SerializeField] InputActionReference _fourDirectionActions;
    void Awake()
    {
        mapSize = 4;
        instance = this;
        fourDirectionActions = _fourDirectionActions.action;
        retry = _retry;
    }
    [SerializeField] SceneReference viaScene;
    [SerializeField] InputActionReference _retry;
    InputAction retry;
    void Start()
    {
        phase = Phase.modeling;
        isGaming = true;
        massMap = AllMass.instance;
        fourMove[0] = massMap.OnPlayerUp;
        fourMove[1] = massMap.OnPlayerLeft;
        fourMove[2] = massMap.OnPlayerDown;
        fourMove[3] = massMap.OnPlayerRight;
        MapAngle = 0;
        StartCoroutine(Control2D());
        StartCoroutine(GameLoop());
    }
    [SerializeField] AudioClip playerSound;
    readonly List<Vector2Int> playerRoute = new();
    public static Vector2Int PlayerPosition
    {
        set
        {
            if (value != AllMass.notExistsPos) instance.playerRoute.Add(value);
            instance.playerSound.PlayRandomPitchSound(instance.gameObject);
            instance.massMap.player.NowPosition = value;
        }
        get => instance.massMap.player.NowPosition;
    }
    List<Vector2Int> modelRoot = new();
    List<Vector2Int> dummyRoot = new();
    float modelInterval = 0.4f;
    int harass = 0;
    [SerializeField] int difficulty = 1;
    public bool playerTracing;
    [SerializeField] List<ScoreDisplay> scoreDisplays = new();
    [SerializeField] PauseButton pauseButton;
    int score = 0;
    int Score
    {
        set
        {
            scoreDisplays.ForEach(element => element.Score(value));
            score = value;
            if (value < -1000) GameOver();
        }
        get => score;
    }
    [SerializeField] List<CompleteCounter> completeCounts = new();
    int completeCount = 0;
    int CompleteCount
    {
        set
        {
            completeCounts.ForEach(element => element.Score(value));
            completeCount = value;
            if (value >= 16) GameClear();
        }
        get => completeCount;
    }
    EventHandlerDisposable p;
    [SerializeField] SceneReference gameClearScene;
    [SerializeField] SceneReference gameOverScene;
    void GameClear()
    {
        PlayerPrefs.SetInt(KeyList.scoreKey, Score);
        GameEnd();
        gameClearScene.LoadScene();
    }
    void GameOver()
    {
        GameEnd();
        gameOverScene.LoadScene();
    }
    /// <summary>
    /// ゲームが終わるときの共通処理
    /// </summary>
    void GameEnd()
    {

    }
    public static int EvaluateListSimilarity<Type>(List<Type> list1, List<Type> list2)
    {
        int score = 0;
        List<Type> smallerList = list1.Count < list2.Count ? list1 : list2;
        List<Type> biggerList = list1.Count >= list2.Count ? list1 : list2;

        // 要素と添字が一致しているか、または要素だけが一致しているかを確認
        for (int i = 0; i < smallerList.Count; i++)
        {
            if (smallerList[i].Equals(biggerList[i]))
            {
                score += 5;
            }
            else if (smallerList.Contains(biggerList[i]) || biggerList.Contains(smallerList[i]))
            {
                score += 3;
            }
            else
            {
                score -= 10;
            }
        }
        for (int i = smallerList.Count; i < biggerList.Count; i++)
        {
            if (smallerList.Contains(biggerList[i])) score += 3;
        }
        // 添字が一致して要素が異なる場合の減点
        for (int i = 0; i < smallerList.Count; i++)
        {
            if (!smallerList[i].Equals(biggerList[i]))
            {
                score -= 3;
            }
        }

        // リストのサイズ差による減点
        score -= 10 * (biggerList.Count - smallerList.Count);

        return score;

    }
    [SerializeField] AudioClip modelSound;
    [SerializeField] AudioClip completeSound;
    IEnumerator GameLoop()
    {
        bool CheckHarass(Harass harass) => ((Harass)this.harass & harass) != 0;
        while (true)
        {
            scoreDisplays.ForEach(element => element.PhaseText());
            completeCounts.ForEach(element => element.PhaseText());
            pauseButton.PhaseColor();
            switch (phase)
            {
                case Phase.modeling:
                    playerRoute.Clear();
                    massMap.AllPhase();
                    harass = UnityEngine.Random.Range(0, difficulty);
                    Camera.main.backgroundColor = Color.white;
                    if (CheckHarass(Harass.bigger) && MapSize != 7)
                    {
                        MapSize++;
                        yield return WaitGameTime(1.0f);
                    }
                    modelRoot = pathGenerator.GenerateRandomPath(MapSize);
                    massMap.model.willFade = false;
                    if (CheckHarass(Harass.fading))
                    {
                        massMap.model.willFade = true;
                        massMap.player.willFade = true;
                    }
                    if (CheckHarass(Harass.dummy))
                    {
                        dummyRoot = pathGenerator.GenerateRandomPath(MapSize);
                        massMap.dummy.Set(new(massMap.defaultDummyColor.r + UnityEngine.Random.Range(-0.1f, 0.1f),
                                                massMap.defaultDummyColor.g + UnityEngine.Random.Range(-0.1f, 0.1f),
                                                massMap.defaultDummyColor.b + UnityEngine.Random.Range(-0.1f, 0.1f)));
                        StartCoroutine(DummyCoroutine());
                        IEnumerator DummyCoroutine()
                        {
                            foreach (Vector2Int path in dummyRoot)
                            {
                                yield return WaitGameTime(modelInterval * (float)(modelRoot.Count) / (float)(dummyRoot.Count));
                                modelSound.PlayRandomPitchSound(gameObject);
                                massMap.dummy.NowPosition = path;
                            }
                        }
                    }
                    foreach (Vector2Int path in modelRoot)
                    {
                        yield return WaitGameTime(modelInterval);
                        modelSound.PlayRandomPitchSound(gameObject);
                        if (difficulty > 5 && UnityEngine.Random.Range(0, 5) == 0)
                        {
                            massMap.model.willFade = true;
                        }
                        massMap.model.NowPosition = path;
                    }
                    yield return WaitGameTime(modelInterval);
                    phase = Phase.tracing;
                    break;
                case Phase.tracing:
                    AllMass.CanClick = true;
                    Camera.main.backgroundColor = Color.black;
                    massMap.AllPhase();
                    if (CheckHarass(Harass.rotate45)) MapAngle += 45 * ((UnityEngine.Random.Range(0, 2) == 0) ? 1 : -1);
                    if (CheckHarass(Harass.rotate90)) MapAngle += 90 * ((UnityEngine.Random.Range(0, 2) == 0) ? 1 : -1);
                    IEnumerator RotateInTracing(int angle)
                    {
                        yield return WaitGameTime(UnityEngine.Random.Range(1f, 10f));
                        MapAngle += angle * ((UnityEngine.Random.Range(0, 2) == 0) ? 1 : -1);
                    }
                    if (CheckHarass(Harass.rotateInTracing45))
                    {
                        StartCoroutine(RotateInTracing(45));
                    }
                    if (CheckHarass(Harass.rotateInTracing90))
                    {
                        StartCoroutine(RotateInTracing(90));
                    }
                    yield return new WaitWhile(() => phase == Phase.tracing);
                    AllMass.CanClick = false;
                    CanMove = false;
                    Score += EvaluateListSimilarity(playerRoute, modelRoot);
                    if (playerRoute.SequenceEqual(modelRoot))
                    {
                        yield return WaitGameTime(0.1f);
                        scoreDisplays.ForEach(element => element.Complete());
                        Score *= 2;
                        CompleteCount++;
                        completeSound.PlayRandomPitchSound(gameObject);
                    }
                    modelInterval *= 0.99f;
                    difficulty++;
                    break;
            }
            yield return WaitGame();
        }
    }
    AllMass massMap;
    readonly Action[] fourMove = new Action[4];
    int mapAngle;
    int easyAngle;
    bool isAngle45;
    bool isAngle315;
    int MapAngle
    {
        set
        {
            scoreDisplays.ForEach((element) => element.Rotate(MapAngle - value));
            completeCounts.ForEach((element) => element.Rotate(MapAngle - value));
            pauseButton.Rotate(MapAngle - value);
            DOTween.Sequence().Append(massMap.transform.DORotate(new(0, 0, -1 * value), 2f).SetEase(Ease.InOutBack))
                                .JoinCallback(() => CanMove = false)
                                .AppendCallback(() => CanMove = true)
                                .SetDependency(() => isGaming);
            if (value % 90 == 0)
            {
                easyAngle = value / 90;
                OnUpArrow = fourMove[((0 + easyAngle) % 4) >= 0 ? (0 + easyAngle) % 4 : (0 + easyAngle) % 4 + 4];
                OnLeftArrow = fourMove[((1 + easyAngle) % 4) >= 0 ? (1 + easyAngle) % 4 : (1 + easyAngle) % 4 + 4];
                OnDownArrow = fourMove[((2 + easyAngle) % 4) >= 0 ? (2 + easyAngle) % 4 : (2 + easyAngle) % 4 + 4];
                OnRightArrow = fourMove[((3 + easyAngle) % 4) >= 0 ? (3 + easyAngle) % 4 : (3 + easyAngle) % 4 + 4];
            }
            else
            {
                int normalizedCurrentAngle = (MapAngle % 360) >= 0 ? MapAngle % 360 : MapAngle % 360 + 360;
                if (value - MapAngle == 90)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (normalizedCurrentAngle == 90 * i + 45 && OnUpArrow == fourMove[i])
                        {
                            easyAngle++;
                            break;
                        }
                    }
                }
                if (value - MapAngle == -90)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (normalizedCurrentAngle == 90 * i - 45 && OnUpArrow == fourMove[i])
                        {
                            easyAngle--;
                            break;
                        }
                        if (i == 0 && normalizedCurrentAngle == 270 && OnUpArrow == fourMove[i])
                        {
                            easyAngle--;
                            break;
                        }
                    }
                }
                OnUpArrow = fourMove[((0 + easyAngle) % 4) >= 0 ? (0 + easyAngle) % 4 : (0 + easyAngle) % 4 + 4];
                OnLeftArrow = fourMove[((1 + easyAngle) % 4) >= 0 ? (1 + easyAngle) % 4 : (1 + easyAngle) % 4 + 4];
                OnDownArrow = fourMove[((2 + easyAngle) % 4) >= 0 ? (2 + easyAngle) % 4 : (2 + easyAngle) % 4 + 4];
                OnRightArrow = fourMove[((3 + easyAngle) % 4) >= 0 ? (3 + easyAngle) % 4 : (3 + easyAngle) % 4 + 4];
            }
            mapAngle = value;
        }
        get => mapAngle;
    }
    Action OnUpArrow;//0度で[0],90度で[1],180度で[2],270度で[3]
    Action OnLeftArrow;//0度で[1]
    Action OnDownArrow;//0度で[2]
    Action OnRightArrow;//0度で[3]
    static bool canMove = false;
    public static bool CanMove
    {
        set => canMove = value;
        get => isGaming && (phase == Phase.tracing) && !AllMass.CanClick && canMove;
    }
    Vector2 inputValue;
    IEnumerator Control2D()
    {
        while (true)
        {
            if (CanMove)
            {
                inputValue = fourDirectionActions.ReadValue<Vector2>();
                if (inputValue.x != 0 || inputValue.y != 0)
                {
                    if (inputValue.x > 0)
                    {
                        OnRightArrow();
                    }
                    else if (inputValue.x < 0)
                    {
                        OnLeftArrow();
                    }
                    if (inputValue.y > 0)
                    {
                        OnUpArrow();
                    }
                    else if (inputValue.y < 0)
                    {
                        OnDownArrow();
                    }
                    yield return WaitGameTime(0.15f);
                }
            }
            yield return WaitGameTime(0.01f);
        }
    }
    public void Retry()
    {
        viaScene.LoadScene();
    }
    protected override void Update()
    {
        base.Update();
    }

    void OnEnable()
    {
        fourDirectionActions.Enable();

        retry.Enable();
        retry.performed += (context) => Retry();
    }


    void OnDisable()
    {
        fourDirectionActions.Disable();

        retry.Disable();
        retry.performed -= (context) => Retry();
    }
}
public class RandomPathGenerator
{
    public enum Edge { Top, Left, Bottom, Right };
    public enum Direction { Up, Left, Down, Right };

    public List<Vector2Int> GenerateRandomPath(int gridSize)
    {
        int verticalLimit = gridSize;
        // スタート地点とゴール地点を決定
        Edge startEdge = (Edge)UnityEngine.Random.Range(0, 4);
        Edge goalEdge = (Edge)(((int)startEdge + 2) % 4);
        Vector2Int startPosition = GetRandomPositionOnEdge(startEdge, gridSize);

        // 主方向を決定
        Direction mainDirection = GetMainDirection(startEdge);

        // 経路を生成
        List<Vector2Int> path = new() { startPosition };
        Vector2Int currentPosition = startPosition;
        Direction lastDirection = mainDirection; // 初期値は任意

        while (!CheckAtEdge(currentPosition, goalEdge, gridSize))
        {
            // 次の移動方向を決定
            Direction nextDirection;
            if (!(CheckAtEdge(currentPosition, (Edge)(((int)startEdge + 1) % 4), gridSize)
            || CheckAtEdge(currentPosition, (Edge)(((int)startEdge + 3) % 4), gridSize)))//垂直方向の端にいないとき
            {
                nextDirection = GetNextDirection(mainDirection, lastDirection);
            }
            else//垂直方向の端にいるとき
            {
                if (lastDirection != mainDirection) nextDirection = mainDirection;//直前に端に到達したなら主方向に移動
                else
                {
                    bool willMoveVertical = UnityEngine.Random.Range(0, 2) == 0;
                    if (CheckAtEdge(currentPosition, (Edge)(((int)startEdge + 1) % 4), gridSize))
                    {
                        nextDirection = willMoveVertical ? (Direction)(((int)startEdge + 3) % 4)
                                                    : mainDirection;
                    }
                    else//CheckAtEdge(currentPosition, (Edge)(((int)startEdge + 3) % 4), gridSize)
                    {
                        nextDirection = willMoveVertical ? (Direction)(((int)startEdge + 1) % 4)
                                                   : mainDirection;
                    }
                }
            }
            if (nextDirection == (Direction)(((int)mainDirection + 1) % 4) || nextDirection == (Direction)(((int)mainDirection + 3) % 4))
            {
                if (verticalLimit == 0)
                {
                    nextDirection = mainDirection;
                }
                else
                {
                    verticalLimit--;
                }
            }
            // 移動
            currentPosition += GetDirectionVector(nextDirection);
            path.Add(currentPosition);
            lastDirection = nextDirection;
        }
        return path;
    }
    bool CheckAtEdge(Vector2Int position, Edge edge, int gridSize)
    {
        return edge switch
        {
            Edge.Top => position.y == 0,
            Edge.Bottom => position.y == gridSize - 1,
            Edge.Right => position.x == gridSize - 1,
            Edge.Left => position.x == 0,
            _ => false
        };
    }
    Vector2Int GetRandomPositionOnEdge(Edge edge, int gridSize)
    {
        Vector2Int position = new();
        switch (edge)
        {
            case Edge.Top:
                position.x = UnityEngine.Random.Range(0, gridSize);
                position.y = 0;
                break;
            case Edge.Bottom:
                position.x = UnityEngine.Random.Range(0, gridSize);
                position.y = gridSize - 1;
                break;
            case Edge.Left:
                position.x = 0;
                position.y = UnityEngine.Random.Range(0, gridSize);
                break;
            case Edge.Right:
                position.x = gridSize - 1;
                position.y = UnityEngine.Random.Range(0, gridSize);
                break;
        }
        return position;
    }
    Direction GetMainDirection(Edge startEdge)
    {
        return startEdge switch
        {
            Edge.Top => Direction.Down,
            Edge.Bottom => Direction.Up,
            Edge.Left => Direction.Right,
            Edge.Right => Direction.Left,
            _ => (Direction)4
        };
    }
    Direction GetNextDirection(Direction mainDirection, Direction lastDirection)
    {
        // 主方向に進む
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            return mainDirection;
        }

        // 垂直方向に進む
        if (mainDirection != lastDirection)
        {
            return lastDirection;
        }
        else//mainDirection == lastVerticalDirection
        {
            bool randomVertical = UnityEngine.Random.Range(0, 2) == 0;
            return mainDirection switch
            {
                Direction.Up or Direction.Down => randomVertical ? Direction.Left : Direction.Right,
                Direction.Left or Direction.Right => randomVertical ? Direction.Up : Direction.Down,
                _ => (Direction)4,// デフォルト
            };
        }
    }
    Vector2Int GetDirectionVector(Direction direction)
    {
        return direction switch
        {
            Direction.Up => new Vector2Int(0, -1),
            Direction.Down => new Vector2Int(0, 1),
            Direction.Left => new Vector2Int(-1, 0),
            Direction.Right => new Vector2Int(1, 0),
            _ => Vector2Int.zero,
        };
    }
}