using System.Collections;
using DG.Tweening;
using Eflatun.SceneReference;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClearScene : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreBoard;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI highScoreBoard;
    [SerializeField] TextMeshProUGUI newRecordBoard;
    [SerializeField] InputActionReference _anyKey;
    InputAction anyKey;
    [SerializeField] Material fontMaterialOutline;
    void Start()
    {
        anyKey = _anyKey.action;
        int currentHighScore = PlayerPrefs.GetInt(KeyList.highScoreKey);
        highScoreBoard.text = currentHighScore.ToString();
        int score = PlayerPrefs.GetInt(KeyList.scoreKey);
        scoreBoard.text = score.ToString();
        scoreBoard.color = new(scoreBoard.color.r, scoreBoard.color.g, scoreBoard.color.b, 0);
        scoreText.color = new(scoreText.color.r, scoreText.color.g, scoreText.color.b, 0);
        newRecordBoard.color = new(newRecordBoard.color.r, newRecordBoard.color.g, newRecordBoard.color.b, 0);
        StartCoroutine(ClearCoroutine());
        IEnumerator ClearCoroutine()
        {
            yield return new WaitForSeconds(0.2f);
            yield return DOTween.Sequence().Append(scoreBoard.DOFade(1.0f, 0.3f))
                                            .Join(scoreText.DOFade(1.0f, 0.3f))
                                            .WaitForCompletion();
            if (score > currentHighScore)
            {
                yield return newRecordBoard.DOFade(1.0f, 0.3f).WaitForCompletion();
                int highScoreBoardValue = currentHighScore;
                Tween highScoreBoardChange = DOTween.To(() => currentHighScore, (x) => highScoreBoardValue = x, score, 0.7f);
                highScoreBoardChange.Pause();
                yield return highScoreBoardChange
                                    .OnUpdate(() =>
                                    {
                                        highScoreBoard.text = highScoreBoardValue.ToString();
                                        if (anyKey.IsPressed())
                                        {
                                            highScoreBoardChange.Complete();
                                        }
                                    })
                                    .Play().WaitForCompletion();
                highScoreBoard.fontSharedMaterial = fontMaterialOutline;
                highScoreBoard.color = newRecordBoard.color;
            }

        }
    }
    [SerializeField] SceneReference GameScene;
    public void Retry()
    {
        GameScene.LoadScene();
    }
    // public void Quit()
    // {
    //     Application.Quit();
    // }
    [SerializeField] SceneReference titleScene;
    public void Quit()
    {
        titleScene.LoadScene();
    }
}
