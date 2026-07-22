using System;
using System.Drawing;
using System.Windows.Forms;
using SnakeGame.Models;
using SnakeGame.Services;

namespace SnakeGame
{
    /// <summary>
    /// Main Window for the Snake Game.
    /// Handles User Interface, Keyboard Inputs, Double Buffered Canvas Rendering,
    /// Game Timer Tick Events, and Save/Continue Workflow.
    /// </summary>
    public partial class FormMain : Form
    {
        // Core Game Constants
        private const int GridSize = 20;     // Grid cell size in pixels
        private const int BoardWidth = 25;   // 25 columns
        private const int BoardHeight = 25;  // 25 rows

        // Game Entities and State
        private Snake snake;
        private Food food;
        private Timer gameTimer;
        private SaveLoadManager saveLoadManager;

        private int score;
        private int highScore;
        private bool isPaused;
        private bool isGameOver;

        public FormMain()
        {
            InitializeComponent();
            SetupDoubleBuffering();

            saveLoadManager = new SaveLoadManager();
            highScore = saveLoadManager.LoadHighScore();

            InitializeGameTimer();
            StartNewGame();
        }

        /// <summary>
        /// Enables Double Buffering on the PictureBox/Form to prevent screen flicker during rendering.
        /// </summary>
        private void SetupDoubleBuffering()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint |
                           ControlStyles.UserPaint |
                           ControlStyles.OptimizedDoubleBuffer, true);
            this.UpdateStyles();
        }

        /// <summary>
        /// Initializes the Windows Forms Game Loop Timer.
        /// </summary>
        private void InitializeGameTimer()
        {
            gameTimer = new Timer();
            gameTimer.Interval = 180; // Speed: 180ms per tick (slowed down)
            gameTimer.Tick += GameTimer_Tick;
        }

        /// <summary>
        /// Resets and starts a brand new game session.
        /// </summary>
        private void StartNewGame()
        {
            snake = new Snake(BoardWidth / 2, BoardHeight / 2);
            food = new Food(BoardWidth, BoardHeight, snake.Body);

            score = 0;
            isPaused = false;
            isGameOver = false;

            UpdateUIStatus("Game Started! Use Arrow Keys to Move.");
            gameTimer.Start();
            picCanvas.Invalidate();

            // IMPORTANT: move keyboard focus to the canvas so arrow keys
            // aren't consumed by whichever button was just clicked.
            picCanvas.Focus();
        }

        /// <summary>
        /// Main Game Loop Tick executed on every timer interval.
        /// </summary>
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (isPaused || isGameOver) return;

            // 1. Move Snake Forward
            snake.Move();

            // 2. Collision Check: Wall Collision
            Point head = snake.Head;
            if (head.X < 0 || head.X >= BoardWidth || head.Y < 0 || head.Y >= BoardHeight)
            {
                TriggerGameOver("Ouch! You hit the wall boundary.");
                return;
            }

            // 3. Collision Check: Self Collision
            if (snake.CheckSelfCollision())
            {
                TriggerGameOver("Oops! You crashed into your own tail.");
                return;
            }

            // 4. Food Eating Check
            if (head.X == food.Position.X && head.Y == food.Position.Y)
            {
                snake.Grow();
                score += 10;

                if (score > highScore)
                {
                    highScore = score;
                    saveLoadManager.SaveHighScore(highScore);
                }

                food.Respawn(BoardWidth, BoardHeight, snake.Body);
            }

            // Update Status and Redraw Canvas
            UpdateUIStatus("Playing...");
            picCanvas.Invalidate();
        }

        /// <summary>
        /// Handles Game Over state transitions.
        /// </summary>
        private void TriggerGameOver(string causeMessage)
        {
            gameTimer.Stop();
            isGameOver = true;
            UpdateUIStatus($"Game Over: {causeMessage}");
            picCanvas.Invalidate();

            MessageBox.Show(
                $"{causeMessage}\n\nFinal Score: {score}\nHigh Score: {highScore}\n\nClick 'New Game' or press Enter to try again!",
                "Game Over",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        /// <summary>
        /// Intercepts arrow keys BEFORE WinForms treats them as dialog/focus-navigation keys.
        /// Without this override, arrow presses get "eaten" by whichever Button (New Game,
        /// Save, Continue, Pause) currently has focus, and never reach the game at all.
        /// This is the actual fix for "arrow keys not working."
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                    snake.ChangeDirection(Direction.Up);
                    return true;
                case Keys.Down:
                    snake.ChangeDirection(Direction.Down);
                    return true;
                case Keys.Left:
                    snake.ChangeDirection(Direction.Left);
                    return true;
                case Keys.Right:
                    snake.ChangeDirection(Direction.Right);
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Keyboard KeyDown Event Handler for W/A/S/D, Pause, and Quick Start/Restart.
        /// Arrow keys are now handled in ProcessCmdKey above (kept here as a harmless
        /// fallback in case ProcessCmdKey doesn't fire in some edge case).
        /// </summary>
        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    snake.ChangeDirection(Direction.Up);
                    break;
                case Keys.Down:
                case Keys.S:
                    snake.ChangeDirection(Direction.Down);
                    break;
                case Keys.Left:
                case Keys.A:
                    snake.ChangeDirection(Direction.Left);
                    break;
                case Keys.Right:
                case Keys.D:
                    snake.ChangeDirection(Direction.Right);
                    break;
                case Keys.P:
                case Keys.Space:
                    TogglePauseGame();
                    break;
                case Keys.N:
                    StartNewGame();
                    break;
                case Keys.Enter:
                    if (isGameOver) StartNewGame();
                    break;
            }
        }

        /// <summary>
        /// Custom Paint Event Handler for drawing the Game Grid, Snake, Food, and Overlays.
        /// </summary>
        private void picCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Background Fill
            g.Clear(Color.FromArgb(20, 24, 33));

            // Grid Lines (Subtle guide pattern)
            using (Pen gridPen = new Pen(Color.FromArgb(30, 36, 48), 1))
            {
                for (int x = 0; x <= BoardWidth; x++)
                    g.DrawLine(gridPen, x * GridSize, 0, x * GridSize, BoardHeight * GridSize);
                for (int y = 0; y <= BoardHeight; y++)
                    g.DrawLine(gridPen, 0, y * GridSize, BoardWidth * GridSize, y * GridSize);
            }

            // Draw Food (Luminous Apple / Food Item)
            if (food != null)
            {
                Rectangle foodRect = new Rectangle(food.Position.X * GridSize + 2, food.Position.Y * GridSize + 2, GridSize - 4, GridSize - 4);
                using (Brush foodBrush = new SolidBrush(Color.FromArgb(239, 68, 68))) // Crimson Red
                {
                    g.FillEllipse(foodBrush, foodRect);
                }
            }

            // Draw Snake Body
            if (snake != null)
            {
                for (int i = 0; i < snake.Body.Count; i++)
                {
                    Point pt = snake.Body[i];
                    Rectangle segmentRect = new Rectangle(pt.X * GridSize + 1, pt.Y * GridSize + 1, GridSize - 2, GridSize - 2);

                    if (i == 0)
                    {
                        // Snake Head: Vibrant Emerald Green
                        using (Brush headBrush = new SolidBrush(Color.FromArgb(16, 185, 129)))
                        {
                            g.FillRectangle(headBrush, segmentRect);
                        }
                    }
                    else if (i == snake.Body.Count - 1)
                    {
                        // Snake Tail: Fiery Flame Effect (Orange/Red Base with Flame Sparks)
                        using (Brush fireBaseBrush = new SolidBrush(Color.FromArgb(249, 115, 22))) // Vibrant Fire Orange
                        using (Brush flameYellowBrush = new SolidBrush(Color.FromArgb(250, 204, 21))) // Yellow Core
                        using (Brush fireRedPen = new SolidBrush(Color.FromArgb(239, 68, 68))) // Crimson Flame Tip
                        {
                            // Outer Fire Base
                            g.FillRectangle(fireBaseBrush, segmentRect);

                            // Inner Hot Yellow Core
                            Rectangle flameCore = new Rectangle(segmentRect.X + 3, segmentRect.Y + 3, segmentRect.Width - 6, segmentRect.Height - 6);
                            g.FillRectangle(flameYellowBrush, flameCore);

                            // Fiery Ember Sparks on Tail Tip
                            g.FillEllipse(fireRedPen, segmentRect.X - 2, segmentRect.Y - 2, 6, 6);
                            g.FillEllipse(fireRedPen, segmentRect.X + segmentRect.Width - 3, segmentRect.Y - 2, 6, 6);
                            g.FillEllipse(fireRedPen, segmentRect.X + 5, segmentRect.Y + segmentRect.Height - 3, 5, 5);
                        }
                    }
                    else
                    {
                        // Snake Body: Gradient Teal Green
                        using (Brush bodyBrush = new SolidBrush(Color.FromArgb(52, 211, 153)))
                        {
                            g.FillRectangle(bodyBrush, segmentRect);
                        }
                    }
                }
            }

            // Pause / Game Over Visual Overlay
            if (isPaused)
            {
                DrawOverlayText(g, "GAME PAUSED", "Press Space/P or 'Continue' to Resume", Color.Khaki);
            }
            else if (isGameOver)
            {
                DrawOverlayText(g, "GAME OVER", $"Final Score: {score} | Press 'New Game' to play again", Color.LightCoral);
            }
        }

        private void DrawOverlayText(Graphics g, string title, string subtitle, Color titleColor)
        {
            using (Brush darkDim = new SolidBrush(Color.FromArgb(180, 0, 0, 0)))
            {
                g.FillRectangle(darkDim, 0, 0, picCanvas.Width, picCanvas.Height);
            }

            using (Font titleFont = new Font("Segoe UI", 22, FontStyle.Bold))
            using (Font subFont = new Font("Segoe UI", 11, FontStyle.Regular))
            using (Brush titleBrush = new SolidBrush(titleColor))
            using (Brush subBrush = new SolidBrush(Color.White))
            {
                SizeF titleSize = g.MeasureString(title, titleFont);
                SizeF subSize = g.MeasureString(subtitle, subFont);

                g.DrawString(title, titleFont, titleBrush, (picCanvas.Width - titleSize.Width) / 2, (picCanvas.Height / 2) - 30);
                g.DrawString(subtitle, subFont, subBrush, (picCanvas.Width - subSize.Width) / 2, (picCanvas.Height / 2) + 15);
            }
        }

        /// <summary>
        /// Updates the HUD Labels and Status Strip.
        /// </summary>
        private void UpdateUIStatus(string statusMsg)
        {
            lblScore.Text = $"Score: {score}";
            lblHighScore.Text = $"High Score: {highScore}";
            lblStatus.Text = statusMsg;
        }

        /// <summary>
        /// Menu Strip: New Game Option Click
        /// </summary>
        private void btnNewGame_Click(object sender, EventArgs e)
        {
            StartNewGame();
        }

        /// <summary>
        /// Menu Strip / Button: Toggle Pause / Resume
        /// </summary>
        private void TogglePauseGame()
        {
            if (isGameOver) return;

            isPaused = !isPaused;
            if (isPaused)
            {
                gameTimer.Stop();
                UpdateUIStatus("Game Paused.");
            }
            else
            {
                gameTimer.Start();
                UpdateUIStatus("Resumed Playing...");
            }
            picCanvas.Invalidate();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            TogglePauseGame();
            picCanvas.Focus();
        }

        /// <summary>
        /// Save Game State to Disk
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (isGameOver)
            {
                MessageBox.Show("Cannot save a completed game over state. Start a new game first!", "Save Game", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool pausedBeforeSave = isPaused;
            if (!isPaused) TogglePauseGame();

            GameState currentSave = new GameState
            {
                SnakeBody = snake.Body,
                CurrentDirection = snake.CurrentDirection,
                FoodPosition = food.Position,
                Score = score,
                HighScore = highScore,
                Timestamp = DateTime.Now.ToString("g")
            };

            bool success = saveLoadManager.SaveGame(currentSave);
            if (success)
            {
                MessageBox.Show("Game session saved successfully!", "Game Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btnContinue.Enabled = true;
            }
            else
            {
                MessageBox.Show("Failed to save game session.", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (!pausedBeforeSave) TogglePauseGame();
            picCanvas.Focus();
        }

        /// <summary>
        /// Continue / Load Saved Game State from Disk
        /// </summary>
        private void btnContinue_Click(object sender, EventArgs e)
        {
            GameState saved = saveLoadManager.LoadGame();
            if (saved == null || saved.SnakeBody == null || saved.SnakeBody.Count == 0)
            {
                MessageBox.Show("No valid saved game file found.", "Continue Game", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            snake = new Snake(saved.SnakeBody, saved.CurrentDirection);
            food = new Food(saved.FoodPosition);
            score = saved.Score;
            highScore = Math.Max(saved.HighScore, highScore);

            isGameOver = false;
            isPaused = false;

            gameTimer.Start();
            UpdateUIStatus($"Loaded game saved on {saved.Timestamp}. Playing!");
            picCanvas.Invalidate();
            picCanvas.Focus();
        }
    }
}