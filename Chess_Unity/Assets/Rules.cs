using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess;
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Rules : MonoBehaviour
{
    DragAndDrop dad;
    Chess.Chess chess;

    public Rules()
    {
        dad = new DragAndDrop();
        chess = new Chess.Chess();
    }
    // Start is called before the first frame update
    void Start()
    {
        ShowFigures();
        if (Menu.netMode == 1)
            ServerStart();
        else
            ClientConnect();
        last = chess.fen;

    }

    string last;
    // Update is called once per frame
    void Update()
    {
        if (netActive==false)
        {
            netActive = true;
            if (Menu.netMode == 1)
            {
                handler = listenSocket.Accept();
                if (chess.fen != "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
                {
                    string temp = ServerReceive();
                    chess = chess.Move(temp);
                    ShowFigures();
                }
            }
            else
            {
                ClientConnect();
                string temp = ClientReceiver();
                chess = chess.Move(temp);
                ShowFigures();
            }
        }
        if (dad.Action())
        {
            if (chess.fen != last)
            {
                last = chess.fen;
            }
            string from = GetSquare(dad.pickPosition);
            string to = GetSquare(dad.dropPosition);
            string figure = chess.GetFigureAt((int)(dad.pickPosition.x / 2.0), (int)(dad.pickPosition.y / 2.0)).ToString();
            string move = figure + from + to;
            chess = chess.Move(move);
            ShowFigures();
            if (chess.fen != last)
            {
                if (Menu.netMode == 1)
                {
                    handler = listenSocket.Accept();
                    ServerSend(move);
                    netActive = false;
                }
                else
                {
                    ClientConnect();
                    ClientSend(move);
                    netActive = false;
                }
            }
            
        }

    }

    string GetSquare(Vector2 position)
    {
        int x = Convert.ToInt32(position.x / 2);
        int y = Convert.ToInt32(position.y / 2);

        //Debug.Log('a' + x);
        return ((char)('a' + x)).ToString()+ (y + 1).ToString();
    }
    void ShowFigures()
    {
        int nr = 0;
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                string figure = chess.GetFigureAt(x,y).ToString();
                //Debug.Log(figure + y + x);
                if (figure == ".") continue;
                PlaceFigure("box" + nr, figure, x, y);
                nr++;
            }
        }
        for (;  nr< 32; nr++)
        {
            PlaceFigure("box" + nr, "q", 9, 9);
        }
    }

    void PlaceFigure(string box, string figure, int x, int y)
    {
        GameObject goBox = GameObject.Find(box);
        GameObject goFigure = GameObject.Find(figure);
        GameObject goSquare = GameObject.Find("" + y + x);

        var spriteFigure = goFigure.GetComponent<SpriteRenderer>();
        var spriteBox = goBox.GetComponent<SpriteRenderer>();
        spriteBox.sprite = spriteFigure.sprite;

        goBox.transform.position = goSquare.transform.position;
    }

    //--------------------------------------------------------------------------------------
    static int port = 8005; // порт дл€ приема вход€щих запросов
    static Socket listenSocket;
    static byte[] data = new byte[256];
    static Socket handler;
    static string address = "127.0.0.1"; // адрес сервера
    static Socket socket;
    static bool netActive = false;

    static void ClientConnect()
    {
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(address), port);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(ipPoint);
    }

    static void ClientSend(string fen)
    {
        string message = fen;
        byte[] data = Encoding.Unicode.GetBytes(message);
        socket.Send(data);
    }

    static string ClientReceiver()
    {
        // получаем ответ
        byte[] data = new byte[256]; // буфер дл€ ответа
        StringBuilder builder = new StringBuilder();
        int bytes = 0; // количество полученных байт

        do
        {
            Reciever();
            data = data1;
            bytes = bytes1;
            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        }
        while (socket.Available > 0);
        return builder.ToString();
    }

    static void ClientClose()
    {
        // закрываем сокет
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }

    static void ServerStart()
    {
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);

        // создаем сокет
        listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        listenSocket.Bind(ipPoint);

        // начинаем прослушивание
        listenSocket.Listen(10);

        Console.WriteLine("—ервер запущен. ќжидание подключений...");
    }

    static async void Reciever()
    {

         bytes1 = await Task.Run(() => handler.Receive(data1, data1.Length, 0));

    }
    static int bytes1;
    static byte[] data1 = new byte[256];
    static string ServerReceive()
    {
        // получаем сообщение
        StringBuilder builder = new StringBuilder();
        int bytes = 0; // количество полученных байтов
        byte[] data = new byte[256]; // буфер дл€ получаемых данных

        do
        {
            Reciever();
            data = data1;
            bytes = bytes1;
            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        }
        while (handler.Available > 0);

        return builder.ToString();
    }

    static void ServerClose()
    {
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();
    }

    static void ServerSend(string fen)
    {
        string message = fen;
        byte[] data = Encoding.Unicode.GetBytes(message);
        handler.Send(data);
    }
}

class DragAndDrop
{
    enum State
    {
        none,
        drag,
    }

    public Vector2 pickPosition { get; private set; }
    public Vector2 dropPosition { get; private set; }
    State state;
    GameObject item;
    Vector2 offset;

    public DragAndDrop()
    {
        this.state = State.none;
        item = null;
    }

    public bool Action()
    {
        switch(state)
        {
            case State.none:
                if (IsMouseButtonPresserd())
                    PickUp();
                break;
            case State.drag:
                if (IsMouseButtonPresserd())
                    Drag();
                else
                {
                    Drop();
                    return true;
                }
                break;
           
        }
        return false;
    }


    bool IsMouseButtonPresserd()
    {
        return Input.GetMouseButton(0);
    }

    void PickUp()
    {
        Vector2 clickPosition = GetClickPosition();
        Transform clickedItem = GetItemAt(clickPosition);
        if (clickedItem == null) return;
        pickPosition = clickedItem.position;
        item = clickedItem.gameObject;
        state = State.drag;
        offset = (Vector2)clickedItem.position - clickPosition;
        
    }

    Vector2 GetClickPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    Transform GetItemAt(Vector2 position)
    {
        RaycastHit2D[] figures = Physics2D.RaycastAll(position, position, 0.5f);
        if (figures.Length == 0)
            return null;
        return figures[0].transform;
    }

    void Drag ()
    {
        item.transform.position = GetClickPosition() + offset;
    }

    void Drop()
    {
        dropPosition = item.transform.position;
        state = State.none;
        item = null;
    }
}
