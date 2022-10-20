using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Арканоид
{
    public partial class Form1 : Form
    {
        const int mapWidth = 20; //ширина карты
        const int mapHeight = 30; //длинна карты

        public int[,] map = new int[mapHeight, mapWidth]; //создание карты 
        public int dirX = 0; //отвечают за направление полета мячик
        public int dirY = 0;
        public int platformX; //те платформы, от которых будет отскакивать мячик
        public int platformY;
        public int ballX;  //позиция мячика
        public int ballY;
        public int score; //текущие очки

        private readonly Timer timer;

        public Image arcanoidSet;

        public Label scoreLabel;
        public Form1()
        {
            InitializeComponent();

            scoreLabel = new Label(); //без понятия какого размера карта, потому что задаю через код, поэтому прописываю программно
            scoreLabel.Location = new Point((mapWidth + 1) * 20, 50); // под ширину карты
            scoreLabel.Text = "Счет: " + score;
            this.Controls.Add(scoreLabel);

            timer = new Timer();
            timer.Tick += new EventHandler(update);//создаем таймер, задаем обработчик событий
            this.KeyUp += new KeyEventHandler(inputCheck); //обработка нажатия клавиши

            Init(); //вызываем функцию инициализации
        }

        private void inputCheck(object sender, KeyEventArgs e)
        {
            map[platformY, platformX] = 0; //очищаем предыдущее место
            map[platformY, platformX + 1] = 0;
            map[platformY, platformX + 2] = 0;
            switch (e.KeyCode)
            {
                case Keys.Right: //обработка события нажатия на клавишу ВПРАВО 
                    if (platformX + 1 < mapWidth - 1) //ограничения, чтобы не заходить за границы карты
                        platformX++;
                    break;
                case Keys.Left: //обработка события нажатия на клавишу ВЛЕВО
                    if (platformX > 0)
                        platformX--;
                    break;
            }
            map[platformY, platformX] = 9;
            map[platformY, platformX + 1] = 99;
            map[platformY, platformX + 2] = 999;
        }

        public void AddLine() //добавляем одну линию для усложнения игры
        {
            for (int i = mapHeight - 2; i > 0; i--)
            {
                for (int j = 0; j < mapWidth; j += 2)
                {
                    map[i, j] = map[i - 1, j];
                }
            }
            Random r = new Random();
            for (int j = 0; j < mapWidth; j += 2)
            {
                int currPlatform = r.Next(1, 5);
                map[0, j] = currPlatform;
                map[0, j + 1] = currPlatform + currPlatform * 10;
            }
        }
        private void update(object sender, EventArgs e)
        {
            if (ballY + dirY > mapHeight - 1) //если столкнулся мячик с нижней границей - вызываем Init и тем самым начинаем новую игру
            {
                Init();
            }

            map[ballY, ballX] = 0; //очищаем предыдущее место мячика
            if (!IsCollide())
                ballX += dirX;
            if (!IsCollide())
                ballY += dirY;
            map[ballY, ballX] = 8; //позиция мячика после изменения координат

            map[platformY, platformX] = 9;
            map[platformY, platformX + 1] = 99;
            map[platformY, platformX + 2] = 999;

            Invalidate(); //перерисовка холста
        }

        public void GeneratePlatforms() //генерируем панельки
        {
            Random r = new Random();
            for (int i = 0; i < mapHeight / 3; i++)
            {
                for (int j = 0; j < mapWidth; j += 2)
                {
                    int currPlatform = r.Next(1, 5);//кол-во очков
                    map[i, j] = currPlatform;
                    map[i, j + 1] = currPlatform + currPlatform * 10;
                }
            }
        }

        public bool IsCollide() //определяет, находится ли мячик в диапозоне
        {
            bool isColliding = false;
            if (ballX + dirX > mapWidth - 1 || ballX + dirX < 0)
            {
                dirX *= -1;
                isColliding = true;
            }
            if (ballY + dirY < 0)//если забегает координата мячика за передлы карты, то изменяем его направление
            {
                dirY *= -1;
                isColliding = true;
            }

            if (map[ballY + dirY, ballX] != 0)//проверка на столкновение с объектами
            {
                bool addScore = false;
                isColliding = true;

                if (map[ballY + dirY, ballX] > 10 && map[ballY + dirY, ballX] < 99)
                {
                    map[ballY + dirY, ballX] = 0;
                    map[ballY + dirY, ballX - 1] = 0;
                    addScore = true;
                }
                else if (map[ballY + dirY, ballX] < 9)
                {
                    map[ballY + dirY, ballX] = 0;
                    map[ballY + dirY, ballX + 1] = 0;
                    addScore = true;
                }
                if (addScore)
                {
                    score += 50;
                    if (score % 200 == 0 && score > 0)
                    {
                        AddLine();
                    }
                }
                dirY *= -1;
            }
            if (map[ballY, ballX + dirX] != 0)
            {
                bool addScore = false;
                isColliding = true;

                if (map[ballY, ballX + dirX] > 10 && map[ballY + dirY, ballX] < 99)
                {
                    map[ballY, ballX + dirX] = 0;
                    map[ballY, ballX + dirX - 1] = 0;
                    addScore = true;
                }
                else if (map[ballY, ballX + dirX] < 9)
                {
                    map[ballY, ballX + dirX] = 0;
                    map[ballY, ballX + dirX + 1] = 0;
                    addScore = true;
                }
                if (addScore)
                {
                    score += 50;
                    if (score % 200 == 0 && score > 0)
                    {
                        AddLine();
                    }
                }
                dirX *= -1;
            }
            scoreLabel.Text = "Счет: " + score;


            return isColliding;
        }

        public void DrawArea(Graphics g) //территория, где все будет происходить 
        {
            g.DrawRectangle(Pens.MidnightBlue, new Rectangle(0, 0, mapWidth * 20, mapHeight * 20));
        }

        public void Init() //функция, в которой будем все инициализировать
        {
            this.Width = (mapWidth + 5) * 20; //будет высчитывать размер окна, которая будет подстраиваться под карту
            this.Height = (mapHeight + 2) * 20;

            arcanoidSet = new Bitmap("D:/3 курс/Коноплев/Арканоид/arcanoidres.png"); 
            timer.Interval = 50;

            score = 0;

            scoreLabel.Text = "Счет: " + score;

            for (int i = 0; i < mapHeight; i++) 
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    map[i, j] = 0; 
                }
            }

            platformX = (mapWidth - 1) / 2; //половина нашей карты (+ ближе к центру)
            platformY = mapHeight - 1; //низ нашей карты

            map[platformY, platformX] = 9; //добавляем на карту платформу. Y - первый аргумент, Х - второй
            map[platformY, platformX + 1] = 99;//чтобы наш мячик распознавал на нашей карте (+хранится часть платформы)
            map[platformY, platformX + 2] = 999;//заполняем и вторую ячейку

            ballY = platformY - 1; //будет находится на платформе
            ballX = platformX + 1; //на середине платформы

            map[ballY, ballX] = 8;

            dirX = 1; //инициализация мячика
            dirY = -1;

            GeneratePlatforms();

            timer.Start();
        } 

        public void DrawMap(Graphics g) //реализация отрисовки объектов
        {
            for (int i = 0; i < mapHeight; i++) 
            {
                for (int j = 0; j < mapWidth; j++)
                {
                    if (map[i, j] == 9)
                    {
                        //если текущий элемент карты = 9, то мы рисуем платформу:
                        g.DrawImage(arcanoidSet, new Rectangle(new Point(j * 20, i * 20), new Size(60, 20)), 398, 17, 150, 50, GraphicsUnit.Pixel);
                    }

                    if (map[i, j] == 8)
                    {
                        //отрисовка мячика
                        g.DrawImage(arcanoidSet, new Rectangle(new Point(j * 20, i * 20), new Size(20, 20)), 806, 548, 73, 73, GraphicsUnit.Pixel);
                    }

                    //отрисовка панелек
                    if (map[i, j] == 1)
                    {

                        g.DrawImage(arcanoidSet, new Rectangle(new Point(j * 20, i * 20), new Size(40, 20)), 20, 16, 170, 59, GraphicsUnit.Pixel);
                    }

                    if (map[i, j] == 2)
                    {
                        g.DrawImage(arcanoidSet, new Rectangle(new Point(j * 20, i * 20), new Size(40, 20)), 20, 16 + 77 * (map[i, j] - 1), 170, 59, GraphicsUnit.Pixel);
                    }

                    if (map[i, j] == 3)
                    {
                        g.DrawImage(arcanoidSet, new Rectangle(new Point(j * 20, i * 20), new Size(40, 20)), 20, 16 + 77 * (map[i, j] - 1), 170, 59, GraphicsUnit.Pixel);
                    }

                    if (map[i, j] == 4)
                    {
                        g.DrawImage(arcanoidSet, new Rectangle(new Point(j * 20, i * 20), new Size(40, 20)), 20, 16 + 77 * (map[i, j] - 1), 170, 59, GraphicsUnit.Pixel);
                    }

                    if (map[i, j] == 5)
                    {
                        g.DrawImage(arcanoidSet, new Rectangle(new Point(j * 20, i * 20), new Size(40, 20)), 20, 16 + 77 * (map[i, j] - 1), 170, 59, GraphicsUnit.Pixel);
                    }
                }
            }
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            DrawArea(e.Graphics);
            DrawMap(e.Graphics);
        }
    }
}
 
