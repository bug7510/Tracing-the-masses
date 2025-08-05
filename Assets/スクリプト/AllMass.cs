using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AllMass : MonoBehaviour
{
    int sideLength;
    public static AllMass instance;
    GridLayoutGroup gridLayoutGroup;
    private void Awake()
    {
        instance = this;
        player = new(playerColor);
        model = new(defaultModelColor);
        dummy = new(defaultDummyColor);
    }
    void Start()
    {
        massMap = new();
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        CanClick = true;
        DesignMap(GameSceneManager.MapSize);
    }
    public void ResizeMap(int sideLength)
    {
        if (sideLength == massMap.Count) return;
        Sequence reSizeWithEffect = DOTween.Sequence()
                                    .Append(transform.DOScale(0, 0.5f))
                                    .AppendCallback(() =>
                                    {
                                        DesignMap(sideLength);
                                        AllPhase();
                                    })
                                    .Append(transform.DOScale(0.08f, 0.5f))
                                    .SetDependency(() => GameTime.isGaming);
    }
    void DesignMap(int sideLength)
    {
        ChangeLayoutGroup(sideLength);
        GenerateMassMap(sideLength);
        this.sideLength = sideLength;
    }

    [SerializeField] EachMass eachMass;
    void ChangeLayoutGroup(int sideLength)
    {
        float cellSize = 90000 / (11 * sideLength + 1);
        int spaceSize = (int)Math.Floor(0.1 * cellSize);

        gridLayoutGroup.padding.left = spaceSize;
        gridLayoutGroup.padding.right = spaceSize;
        gridLayoutGroup.padding.top = spaceSize;
        gridLayoutGroup.padding.bottom = spaceSize;

        gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);

        // 間隔の変更
        gridLayoutGroup.spacing = new Vector2(spaceSize, spaceSize);
    }
    static List<List<EachMass>> massMap = new();
    readonly List<EachMass> disabledMasses = new();
    void GenerateMassMap(int sideLength)
    {
        if (massMap.Count != 0)
        {
            massMap.SelectMany(element => element).ToList()
            .ForEach(element => element.Reset());
        }
        int massNumberDiff = massMap.Count * massMap.Count - sideLength * sideLength;
        List<EachMass> massMap1d = GenerateMassMap1d();
        massMap = ConvertMapDimension1to2(massMap1d);
        List<EachMass> GenerateMassMap1d()
        {
            List<EachMass> returnList = new();
            if (sideLength < massMap.Count)
            {
                disabledMasses.InsertRange(0, massMap.SelectMany(element => element)
                                            .Reverse()
                                            .Take(massNumberDiff)
                                            .Reverse()
                                            .ToList()
                                            .Select(
                                                item =>
                                                {
                                                    item.gameObject.SetActive(false);
                                                    return item;
                                                }));
                returnList = massMap.SelectMany(element => element)
                                    .Where((element) => !disabledMasses.Contains(element))
                                    .ToList();
            }
            else//sideLength > massMap.Count
            {
                returnList = massMap.SelectMany(element => element).ToList();
                massNumberDiff *= -1;
                while (disabledMasses.Count > 0 && massNumberDiff > 0)
                {
                    returnList.Add(disabledMasses[0]);
                    disabledMasses[0].gameObject.SetActive(true);
                    disabledMasses.RemoveAt(0);
                    massNumberDiff--;
                }
                while (massNumberDiff > 0)
                {
                    returnList.Add(Instantiate(eachMass, transform));
                    massNumberDiff--;
                }
            }
            return returnList;
        }
        List<List<EachMass>> ConvertMapDimension1to2(List<EachMass> map1d)
        {
            List<List<EachMass>> returnMap = new();
            int i = 0;
            for (int x = 0; x < sideLength; x++)
            {
                returnMap.Add(new());
                for (int y = 0; y < sideLength; y++)
                {
                    returnMap[x].Add(map1d[i]);
                    map1d[i].Set(x, y, OnEachMassClicked);
                    i++;
                }
            }
            return returnMap;
        }
    }
    static bool canClick;
    public static bool CanClick
    {
        set => canClick = value;
        get => GameTime.isGaming && GameSceneManager.phase == Phase.tracing && canClick;
    }
    public readonly static Vector2Int notExistsPos = new(-1, -1);

    [SerializeField] Color playerColor;
    [SerializeField] Color defaultModelColor;
    public Color defaultDummyColor;
    public InMassObject player;

    public InMassObject model;
    public InMassObject dummy;

    public class InMassObject
    {
        public Color objectColor;
        public bool willFade = false;
        public InMassObject(Color objectColor) => Set(objectColor);
        public void Set(Color objectColor)
        {
            this.objectColor = objectColor;
        }
        Vector2Int nowPosition = notExistsPos;
        public Vector2Int NowPosition
        {
            set
            {
                if (value != nowPosition)
                {
                    if (value != notExistsPos)
                    {
                        massMap[(int)value.x][(int)value.y].ColorChange(objectColor);
                        if (nowPosition != notExistsPos) massMap[nowPosition.x][nowPosition.y].ColorChange(objectColor * 0.8f);
                        if (nowPosition != notExistsPos && willFade)
                        {
                            massMap[nowPosition.x][nowPosition.y].EachFade();
                        }
                    }
                }
                nowPosition = value;
            }
            get => nowPosition;
        }
    }
    void OnEachMassClicked(Vector2Int vector) => OnEachMassClicked((int)vector.x, (int)vector.y);
    void OnEachMassClicked(int x, int y)
    {
        if (CanClick)
        {
            GameSceneManager.PlayerPosition = new(x, y);
            CanClick = false;
            GameSceneManager.CanMove = true;
        }
    }
    public void AllPhase()
    {
        massMap.SelectMany(element => element)
                .ToList()
                .ForEach(element => element.EachPhase());

        GameSceneManager.PlayerPosition = notExistsPos;
        model.NowPosition = notExistsPos;
        dummy.NowPosition = notExistsPos;
    }
    public void OnPlayerLeft()
    {
        if (GameSceneManager.PlayerPosition.y - 1 < 0) { GameSceneManager.phase = Phase.modeling; return; }//ここで端に来た判定
        GameSceneManager.PlayerPosition -= new Vector2Int(0, 1);
    }
    public void OnPlayerRight()
    {
        if (GameSceneManager.PlayerPosition.y + 1 >= sideLength) { GameSceneManager.phase = Phase.modeling; return; }//ここで端に来た判定
        GameSceneManager.PlayerPosition += new Vector2Int(0, 1);
    }
    public void OnPlayerDown()
    {
        if (GameSceneManager.PlayerPosition.x + 1 >= sideLength) { GameSceneManager.phase = Phase.modeling; return; }//ここで端に来た判定
        GameSceneManager.PlayerPosition += new Vector2Int(1, 0);
    }
    public void OnPlayerUp()
    {
        if (GameSceneManager.PlayerPosition.x - 1 < 0) { GameSceneManager.phase = Phase.modeling; return; }//ここで端に来た判定
        GameSceneManager.PlayerPosition -= new Vector2Int(1, 0);
    }
}
