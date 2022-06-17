using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Boot : MonoBehaviour
{

    /// <summary>
    ///  Ce fichier permet d'initialiser le projet Cyber_Cave. 
    ///  Il lance les routines principales n�cessaires au bon d�roulement du jeu et d'initaliser certains scripts. 
    /// </summary>
    public bool ByPassMaster = false;
    public Vector3 CharacterBootPosition = Vector3.zero; //new Vector3(0.351f, 0.94f, 1.267f);
    [Header("Maj Data")]
    public GameObject MAJUI;

    public bool DebugReconnect = false;

    public Coroutine connectionRoutine;
    private GameObject WaitingAudio;

    private void Start()
    {
        // Load scene...
        SceneUpdater.ProccessSaveFile();

        // Add Controller information
        this.gameObject.AddComponent<ControllerData>();
        this.gameObject.AddComponent<HandUtilities>();
        this.gameObject.AddComponent<Character>();
        this.gameObject.AddComponent<RaycastDebugger>();

        // Add anchor Updater and load avatar
        AnchorUpdater Au = this.gameObject.AddComponent<AnchorUpdater>();
        Au.StartCoroutine(Au.TryUpdatingAnchors_Offline());
        // Add character controller script
        // this.gameObject.AddComponent<Character>();

        // Load Scenario Script...
        this.gameObject.AddComponent<EventAndScenario>();

        // Refresh directories
        IOManager.CheckFileDirectories();
        //Get Config file
        Ini.GetConfig();
        // Load INI cache info for PoseRecognition
        PoseRecognition.GetIniFile();

        // Load memory
        BitMemory.LoadMemoriesFromLatestUser();
        GameStatus.SetGameFlag(3); // set user interaction to "transform modification"

        // Run routines
        StartCoroutine(SaveMemory());

        if (!ByPassMaster)
            connectionRoutine = StartCoroutine(ConnectToCyberCaveThroughMaster(false));
        else
            ConnectFastToCyberCave();
       

        // Apply Communication
        if (Ini.GetConfigVariable("GENERAL", "IO_READY") == "TRUE")
        {
            XYPlotting.InitTCPCommunication();
        }

    }

    private void Update()
    {
        if (DebugReconnect)
        {
            DebugReconnect = false;
            NetUtilities.master = null;
            GameStatus.UnsetGameFlag(10);
            GameStatus.UnsetGameFlag(11);
            GameStatus.UnsetGameFlag(12);
            GameStatus.UnsetGameFlag(13);
            // Send UI INFO & leave
            Boot b = Camera.main.transform.root.gameObject.GetComponent<Boot>();
            if (!b.ByPassMaster)
                b.StartCoroutine(b.ConnectToCyberCaveThroughMaster(true));
        }
    }
    public IEnumerator SaveMemory(int repeatRates = 1)
    {
        if (repeatRates < 1)
            repeatRates = 1;
        while (true)
        {
            yield return new WaitForSeconds(repeatRates);
            BitMemory.SaveMemories();
        }
    }

    public void ForceConnectionRoutineToEnd()
    {
        if (WaitingAudio)
            Destroy(WaitingAudio);

        // Force End All dll
        P2SHARE.ForceEndAllDlls();

        try
        {
            if (connectionRoutine != null)
                StopCoroutine(connectionRoutine);
        }
        catch (System.Exception e) { }
    }

    public IEnumerator QuitCyberCaveSafely(string Message)
    {
        AnchorUpdater updater = this.gameObject.GetComponent<AnchorUpdater>();
        // @ Teleport to room somewhere
        transform.position = new Vector3(-35, 156, -35); // default somewhere
        // @ Loading LOL audio
        SoundMap.PlaySoundAndDisposeFromAssets("exitsound");
        // @ Loading Canvas.
        GameObject pseudoscene = MAJUI;
        pseudoscene.SetActive(true);
        GameObject UIObject = ObjectUtilities.FindGameObjectChild(pseudoscene, "UI");
        UIObject.transform.position = transform.position + transform.forward * 5f;
        Vector3 euler = MathUtilities.GetPositionAndEulerInFrontOfPlayer(5f)[1];
        UIObject.GetComponent<RectTransform>().eulerAngles = euler;
        GameObject radialbar = ObjectUtilities.FindGameObjectChild(UIObject, "RadialBar");
        GameObject UIText = ObjectUtilities.FindGameObjectChild(UIObject, "Text");
        radialbar.SetActive(false);
        string s = Message+ "\n";

        string dots = "...";
        int dotctr = 0;
        UIText.GetComponent<TMP_Text>().text = s + dots;

        int ctr = 0;
        while (ctr < 5)
        {
            yield return new WaitForSeconds(1f);
            dotctr++;
            if (dotctr > 5)
                dotctr = 1;
            dots = "";
            for (int i = 0; i < dotctr; i++)
                dots += ".";
            UIText.GetComponent<TMP_Text>().text = s + dots;

            ctr++;
        }
        Quit.Proccess();

    }
    public IEnumerator ConnectToCyberCaveThroughMaster(bool _reconnect)
    {
        // @ Start NetBoot
        AnchorUpdater updater = this.gameObject.GetComponent<AnchorUpdater>();
        if (!_reconnect)
        {
            updater.StartCoroutine(GetComponent<AnchorUpdater>().TryUpdatingAnchors_Online());
            gameObject.AddComponent<NetBoot>();
            // Ungrabbed all object 
            Grabbable[] gbs = FindObjectsOfType<Grabbable>();
            foreach (Grabbable g in gbs)
            {
                if (g._userisgrabber && g._grabbed)
                    g.ForceReleasing();

            }
        }
       

        // @ Teleport to room somewhere
        transform.position = new Vector3(-35, 156, -35); // default somewhere

        // @ Loading LOL audio
        if (!_reconnect)
            WaitingAudio = SoundMap.PlayLoopFromAssets("i am waiting", transform.position);
        else
            WaitingAudio = SoundMap.PlayLoopFromAssets("loadingsound", transform.position);

        // @ Loading Canvas.
        GameObject pseudoscene = MAJUI;
        pseudoscene.SetActive(true);
        GameObject UIObject = ObjectUtilities.FindGameObjectChild(pseudoscene, "UI");
        UIObject.transform.position = transform.position + transform.forward * 5f;
        Vector3 euler = MathUtilities.GetPositionAndEulerInFrontOfPlayer(5f)[1];
        UIObject.GetComponent<RectTransform>().eulerAngles = euler;
        GameObject radialbar  = ObjectUtilities.FindGameObjectChild(UIObject, "RadialBar");
        GameObject UIText = ObjectUtilities.FindGameObjectChild(UIObject, "Text");
        radialbar.SetActive(false);

        // @ Wait while connecting to master
        string s = _reconnect == false ? "connect to master node" + "\n" : "trying to reconnect to master" + "\n";

        string dots = "...";
        int dotctr = 0;
        UIText.GetComponent<TMP_Text>().text = s + dots;

        while (!GameStatus.IsGameFlagSet(10))
        {
            yield return new WaitForSeconds(1f);
            dotctr++;
            if (dotctr > 5)
                dotctr = 1;
            dots = "";
            for (int i = 0; i < dotctr; i++)
                dots += ".";
            UIText.GetComponent<TMP_Text>().text = s + dots;
            if (NetUtilities._mNetStream && !NetStream._imMaster)
            {
                NetUtilities._mNetStream.PingMaster();
            }
        }
        if (GameStatus.IsGameFlagSet(11) || _reconnect) // user is master
        {
            while (!updater.mAvatar)
                yield return new WaitForSeconds(1f);
            Destroy(WaitingAudio);
            pseudoscene.SetActive(false);//Destroy(pseudoscene);
            transform.position = CharacterBootPosition;
            GameStatus.SetGameFlag(13);
            yield break;
        }
        // @ Wait while receiving update to master
        s = "asking for updates" + "\n";
        UIText.GetComponent<TMP_Text>().text = s + dots;
        while (!GameStatus.IsGameFlagSet(12))
        {
            yield return new WaitForSeconds(1f);
            dotctr++;
            if (dotctr > 5)
                dotctr = 1;
            dots = "";
            for (int i = 0; i < dotctr; i++)
                dots += ".";
            UIText.GetComponent<TMP_Text>().text = s + dots;
        }
        // @ Wait while downloading updates
        s = "downloading updates" + "\n";
        radialbar.SetActive(true);
        radialbar.GetComponent<UnityEngine.UI.Image>().fillAmount = 0f;
        while (P2SHARE._dlls.Count > 0)
        {
            yield return new WaitForSeconds(1f);
            dotctr++;
            if (dotctr > 5)
                dotctr = 1;
            dots = "";
            for (int i = 0; i < dotctr; i++)
                dots += ".";
            UIText.GetComponent<TMP_Text>().text = s + dots;

            // @ Set radial bar values $
            // faire la moyenne de tous les prct de cells
            float totalcell = 0;
            float sumprct = 0f;
            foreach (P2SHARE.DL dl in P2SHARE._dlls)
            {
                foreach (P2SHARE.UploadCell c in dl._cells)
                {
                    sumprct += ((float)c.cell_fill_status / (float)c.l) * 100;
                    totalcell++;
                }
               
            }
            sumprct /= totalcell;
            radialbar.GetComponent<UnityEngine.UI.Image>().fillAmount = sumprct / 100;
        }

        while (!updater.mAvatar)
            yield return new WaitForSeconds(1f);

        Destroy(WaitingAudio);
        transform.position = CharacterBootPosition;
        GameStatus.SetGameFlag(13);
        pseudoscene.SetActive(false);
        connectionRoutine = null;
        yield break;
    }

    public void ConnectFastToCyberCave()
    {

        if (GetComponent<AnchorUpdater>())
        {
            GetComponent<AnchorUpdater>().StartCoroutine(GetComponent<AnchorUpdater>().TryUpdatingAnchors_Online());
        }
        this.gameObject.AddComponent<NetBoot>();

    }

}
