using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GallowsGame.Properties;
using Newtonsoft.Json;

namespace GallowsGame
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Список слов по-умолчанию
        /// </summary>
        private List<WordInfo> defaultWordInfos = new List<WordInfo>()
        {
            new WordInfo()
            {
                Word = "Аблютофобия",
                Description = "Боязнь умывания, купания, стирки или чистки",
                Theme = "Фобии"
            },
            new WordInfo()
            {
                Word = "Пневмонит",
                Description = "Интерстициальное воспаление сосудистой стенки альвеол, сопровождающееся их рубцеванием",
                Theme = "Болезни"
            },
            new WordInfo()
            {
                Word = "Петрикор",
                Description = "Запах земли после дождя",
                Theme = "Прочее"
            },
        };
        


        static Random rnd = new Random();

        /// <summary>
        /// Список тем
        /// </summary>
        private List<string> themes = new List<string>();

        /// <summary>
        /// Весь список слов
        /// </summary>
        private List<WordInfo> wordInfos = new List<WordInfo>();

        /// <summary>
        /// Текущее слово
        /// </summary>
        private WordInfo currentWord;

        private int errorCount = 0;

        public Form1()
        {
            // устанавливаем иконку приложения
            this.Icon = Resources.tool;

            InitializeComponent();

            // загружаем слова
            LoadWords();

            InitializeNewGame();
        }


        /// <summary>
        /// Загрузка информации о словах из файла
        /// </summary>
        /// <returns>Возвращает True, если слова успешно загружены, иначе - false</returns>
        private void LoadWords()
        {
            try
            {
                // получаем путь к папке приложения
                var appDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

                // формируем путь к файлу со словами
                var filePath = Path.Combine(appDirectory, "words.json");

                if (!File.Exists(filePath))
                {
                    // Показываем диалоговое окно с возможностью выбора файла, если он не нашелся в корневой папке
                    DialogResult result = MessageBox.Show("Не удалось найти файл со словами!\nВыбрать файл?", "Ошибка", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (result == DialogResult.Yes)
                    {
                        // создаем объект диалогово окна выбора файла с фильтром только по JSON файлам
                        OpenFileDialog dialog = new OpenFileDialog();
                        dialog.Filter = "JSON files |*.json";
                        dialog.Title = "Выберите JSON файл со словами";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            filePath = dialog.FileName;
                        }
                    }
                }

                if(string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                    wordInfos = defaultWordInfos;
                else
                {
                    // считываем весь текст из файла
                    var wordInfosJson = File.ReadAllText(filePath);

                    // преобразуем текст в список объектов
                    wordInfos = JsonConvert.DeserializeObject<List<WordInfo>>(wordInfosJson) ?? new List<WordInfo>();
                }

                // выбираем из списка с информацией о словах все темы и убираем дубли
                themes = wordInfos.Select(x => x.Theme).Distinct().ToList();

                // если есть хотя бы одно слово, то инициализируем темы
                if (wordInfos.Any())
                {
                    // привязываем список тем к ComboBox 
                    BindingSource bindingSource = new BindingSource();
                    bindingSource.DataSource = themes;
                    ThemesComboBox.DataSource = bindingSource.DataSource;

                    // выбираем случайную тему для первой игры
                    int currentThemeIndex = rnd.Next(themes.Count);
                    // проставляем полученную тему в ComboBox
                    ThemesComboBox.SelectedIndex = currentThemeIndex;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Не удалось загрузить файл со словами.\n{e.Message}");
            }
        }

        /// <summary>
        /// Получить случайное слово из списка по выбранной теме
        /// </summary>
        private void GetRandomWord()
        {
            if (wordInfos.Any())
            {
                // отбираем из общего списка только те слова, которые относятся к выбранной теме
                List<WordInfo> currentThemeWordInfos = wordInfos.Where(x => x.Theme == themes[ThemesComboBox.SelectedIndex]).ToList();

                // получаем случайный индекс из массива отобранных   слова
                int currentWordIndex = rnd.Next(currentThemeWordInfos.Count);

                // назначаем новое слово
                currentWord = currentThemeWordInfos[currentWordIndex];
            }
        }

        /// <summary>
        /// Инициализируем все данные для новой игры
        /// </summary>
        private void InitializeNewGame()
        {
            // показываем все кнопки
            ShowAllButtons();

            // получаем случайное слово из списка
            GetRandomWord();

            errorCount = 0;

            // если удалось подобрать слово, то инициализируем все значения
            if (currentWord != null)
            {
                WordLabel.Text = new String('*', currentWord.Word.Length);

                GallowsImage.Image = Resources.Висельница1;
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Новая игра"
        /// </summary>
        /// <param name="sender">Объект кнопки</param>
        /// <param name="e">Аргументы события</param>
        private void NewGameButton_Click(object sender, EventArgs e)
        {

            InitializeNewGame();
        }

        /// <summary>
        /// Обработчик нажатия на кнопку "Правила"
        /// </summary>
        /// <param name="sender">Объект кнопки</param>
        /// <param name="e">Аргументы события</param>
        private void RulesButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Существует набор слов, объединенных между собой по следующим признакам: имя Существительное, нарицательное, Именительного падежа, единственного числа. Среди всех терминов рандомно выбирается лишь одно из них, которое впоследствии потребуется отгадать игроку. А отгадывается слово следующим образом: игрок предлагает букву, которая может содержаться в слове, если такая буква присутствует, то она занимает соответствующую(ие) ей позицию(и). На отгадывание слова дается лишь пять попыток, после совершения шестой ошибки следует “повешение”. Вне зависимости от конечного результата игрок может ознакомиться со значением загаданного ему слова.",
                "Правила", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Обработчик нажатия на кнопку с буквой
        /// </summary>
        /// <param name="sender">Объект кнопки</param>
        /// <param name="e">Аргументы события</param>
        private void CharButton_Click(object sender, EventArgs e)
        {
            // если нет слова для игры - выходим и ничего не делаем
            if (currentWord == null) return;

            if (sender is Button button)
            {
                // скрываем кнопку
                button.Hide();

                // проверяем, есть ли выбранная буква в слове
                if (currentWord.Word.ToUpper().Contains(button.Text))
                {
                    // открываем буквы в слове
                    for (int i = currentWord.Word.ToUpper().IndexOf(button.Text); i > -1; i = currentWord.Word.ToUpper().IndexOf(button.Text, i + 1))
                    {
                        StringBuilder sb = new StringBuilder(WordLabel.Text);
                        sb[i] = Char.Parse(button.Text);
                        WordLabel.Text = sb.ToString();
                    }
                }
                else
                {
                    errorCount++;

                    // в зависимости от номера текущей ошибки устанавливаем соответствующее изображение
                    switch (errorCount)
                    {
                        case 1:
                            GallowsImage.Image = Resources.Висельница2;
                            break;
                        case 2:
                            GallowsImage.Image = Resources.Висельница3;
                            break;
                        case 3:
                            GallowsImage.Image = Resources.Висельница4;
                            break;
                        case 4:
                            GallowsImage.Image = Resources.Висельница5;
                            break;
                        case 5:
                            GallowsImage.Image = Resources.Висельница6;
                            break;
                        case 6:
                            GallowsImage.Image = Resources.Висельница7;
                            break;
                    }

                    // если количество ошибок достигло 6, значит проиграли
                    if (errorCount == 6)
                    {
                        MessageBox.Show($"\nСлово: {currentWord.Word}\nОписание: {currentWord.Description}",
                            "Вы проиграли!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        InitializeNewGame();
                    }
                }

                // если не осталось ни одного знака *, значит выиграли
                if (!WordLabel.Text.Contains("*"))
                {
                    MessageBox.Show($"\nСлово: {currentWord.Word}\nОписание: {currentWord.Description}",
                        "Вы выиграли!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            WordGroup.Focus();
        }

        /// <summary>
        /// Показать все кнопки
        /// </summary>
        private void ShowAllButtons()
        {
            button1.Show();
            button2.Show();
            button3.Show();
            button4.Show();
            button5.Show();
            button6.Show();
            button7.Show();
            button8.Show();
            button9.Show();
            button10.Show();
            button11.Show();
            button12.Show();
            button13.Show();
            button14.Show();
            button15.Show();
            button16.Show();
            button17.Show();
            button18.Show();
            button19.Show();
            button20.Show();
            button21.Show();
            button22.Show();
            button23.Show();
            button24.Show();
            button25.Show();
            button26.Show();
            button27.Show();
            button28.Show();
            button29.Show();
            button30.Show();
            button31.Show();
            button32.Show();
        }
    }
}
