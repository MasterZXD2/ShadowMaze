using ShadowMaze.InteractSystem;
using ShadowMaze.MyCharacterController;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PTBPuzzle : Puzzle
{
    [field: SerializeField] private List<GameObject> placePoints;
    [field: SerializeField] private List<string> placeNames;
    [SerializeField] private float chenkRange = .5f;

    [SerializeField] private GameObject HintBook;
    [SerializeField] private float rangeHB;
    [SerializeField] private GameObject UI;

    [SerializeField] private Image ImageUI;
    [field: SerializeField] private List<Sprite> hintImg;
    [SerializeField] private Button nextBU;
    [SerializeField] private Button prevBU;

    private bool bookOpen = false;

    private GameObject currUI;
    private bool _playerIsInRange = false;

    private int _pagesIndex = 0;
    private int i = 0;
    private List<GameObject> _books = new List<GameObject>();

    protected virtual void Start()
    {
        nextBU.onClick.RemoveAllListeners();
        nextBU.onClick.AddListener(() => { 
            if (!bookOpen) return; 
            if (_pagesIndex < hintImg.Count) _pagesIndex++;
            OnImage(); 
        });
        prevBU.onClick.RemoveAllListeners();
        prevBU.onClick.AddListener(() => { 
            if (!bookOpen) return; 
            if (_pagesIndex > 0) _pagesIndex--; 
            OnImage(); 
        });

        currUI = Instantiate(UI, HintBook.transform);
        currUI.transform.localPosition = Vector3.zero;

        Vector3 scale = currUI.transform.localScale;
        scale.x = 1 + (1 - transform.localScale.x);
        currUI.transform.localScale = scale;

        Canvas canvas = currUI.GetComponentInChildren<Canvas>();

        Vector3 uiOffset = -HintBook.transform.forward * ((HintBook.transform.localScale.z / 2f) - .2f);
        canvas.transform.position = HintBook.transform.position + uiOffset;

        currUI.SetActive(false);
    }

    private void Update()
    {
        PlayerController player = PlayerController.Instance;
        if (!player || !player._playerInteract) return;

        float sqrDist = (player.transform.position - HintBook.transform.position).sqrMagnitude;

        if (sqrDist < rangeHB * rangeHB)
        {
            if (!_playerIsInRange && !player._playerInteract._interactObject)
            {
                player._playerInteract.SetInteractObjectWithOut(name,Interact,currUI);
                _playerIsInRange = true;
                currUI.SetActive(true);
            }
        }
        else
        {
            if (_playerIsInRange && player._playerInteract._interactObject._name == name)
            {
                player._playerInteract.RemoveInteractObject();
                _playerIsInRange = false;
                currUI.SetActive(false);
            }
        }

        i = 0;
        List<GameObject> books = new List<GameObject>();
        for (int j = 0; j < placePoints.Count; j++)
        {
            GameObject go = placePoints[j];
            Collider[] colliders = Physics.OverlapSphere(go.transform.position, chenkRange);
            foreach (Collider collider in colliders)
            {
                GameObject GO = collider.gameObject;
                if (GO.TryGetComponent<BookPuzzleObject>(out BookPuzzleObject bp))
                {
                    if (bp.Name == placeNames[j]) { 
                        i++;
                        books.Add(GO);
                    }
                }
            }
        }

        if (i >= placePoints.Count)
        {
            isFinish = true;
            _books = books;
        }
        else { 
            isFinish = false;
            _books.Clear();
        }
    }

    public override void OnFinish() {
        base.OnFinish();
        foreach (GameObject go in _books) { 
            Destroy(go);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(HintBook.transform.position, rangeHB);
        Gizmos.color = Color.white;
        if (placePoints.Count > 0 ) foreach (GameObject go in placePoints) Gizmos.DrawWireSphere(go.transform.position,chenkRange);
    }

    public virtual void Interact()
    {
        bookOpen = !bookOpen;

        if (bookOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        ImageUI.gameObject.SetActive(bookOpen);
        nextBU.gameObject.SetActive(bookOpen);
        prevBU.gameObject.SetActive(bookOpen);
        OnImage();
    }

    private void OnImage()
    {
        ImageUI.sprite = hintImg[_pagesIndex];
        Debug.Log(ImageUI.sprite);
    }
}
