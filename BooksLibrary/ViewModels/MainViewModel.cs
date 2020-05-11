﻿using System.Collections.ObjectModel;
using System.Threading;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using Seterlund.CodeGuard;
using BooksLibrary.Models;
using System.Windows;

namespace BooksLibrary.ViewModels
{
    

    /// <summary>
    /// MainWindow view model.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly IUIVisualizerService _uiVisualizerService;
        private readonly IPleaseWaitService _pleaseWaitService;
        private readonly IMessageService _messageService;
        ObservableCollection<Book> currCollection;
        public override string Title { get { return "View model title"; } }

        public MainViewModel(IUIVisualizerService uiVisualizerService, IPleaseWaitService pleaseWaitService, IMessageService messageService)
        {
            _uiVisualizerService = uiVisualizerService;
            _pleaseWaitService = pleaseWaitService;
            _messageService = messageService;


            //TODO: Источник данных задан хардкодом.
            //Добавить команды обработки событий "загрузить данные" / "сохранить данные"
            //Использовать по выбору: присоединенный / отсоединенный режимы работы с РБД
            //или Entity Framework DatabaseFirst
            currCollection = new ObservableCollection<Book>();
            BooksCollection = new ObservableCollection<Book>
            {
                new Book {Title = "Автостопом по галактике", Author = "Дуглас Адамс"},
                new Book {Title = "Сто лет одиночества", Author = "Габриель Гарсиа Маркес"},
                new Book {Title = "Маленький принц", Author = "Антуан де Сент-Экзюпери"},
                new Book {Title = "1984", Author = "Джордж Оруэлл"},
                new Book {Title = "Над пропастью во ржи", Author = "Джером Дэвид Сэлинджер"},
            };
            foreach (var item in BooksCollection)
            {
                currCollection.Add(item);
            }
        }

        //тип ObservableCollection используется для отслеживания изменений в коллекции:
        //как только они происходят, выводимые в представление данные обновляются
        public ObservableCollection<Book> BooksCollection
        {
            get { return GetValue<ObservableCollection<Book>>(BooksCollectionProperty); }
            set { SetValue(BooksCollectionProperty, value); }
        }
        public static readonly PropertyData BooksCollectionProperty = RegisterProperty("BooksCollection", typeof(ObservableCollection<Book>));


        public Book SelectedBook
        {
            get { return GetValue<Book>(SelectedBookProperty); }
            set { SetValue(SelectedBookProperty, value); }
        }
        public static readonly PropertyData SelectedBookProperty = RegisterProperty("SelectedBook", typeof(Book));

        
        private Command _addCommand;
        public Command AddCommand
        {
            get
            {
                return _addCommand ?? (_addCommand = new Command(() =>
                {
                    var viewModel = new BookViewModel();

                    _uiVisualizerService.ShowDialog(viewModel, (sender, e) =>
                    {
                        //внутри условия будет получено: если в e.Result - не null, то его значение,
                        //если null, то false
                        if (e.Result ?? false)
                        {
                            BooksCollection.Add(viewModel.BookObject);
                        }
                    });
                    currCollection.Clear();
                    foreach (var item in BooksCollection)
                    {
                        currCollection.Add(item);
                    }
                }));
            }
        }


        private Command _editCommand;
        public Command EditCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = new Command(() =>
                {
                    var viewModel = new BookViewModel(SelectedBook);
                    _uiVisualizerService.ShowDialog(viewModel);
                    currCollection.Clear();
                    foreach (var item in BooksCollection)
                    {
                        currCollection.Add(item);
                    }
                },
                () => SelectedBook != null)); //разрешение на установку св-ва при соблюдении условия
            }
        }


        private Command _removeCommand;
        public Command RemoveCommand
        {
            get
            {
                return _removeCommand ?? (_removeCommand = new Command(async () =>
                {
                    if (await _messageService.Show("Вы действительно хотите удалить объект?", "Внимание!",
                        MessageButton.YesNo, MessageImage.Warning) != MessageResult.Yes)
                    {
                        return;
                    }

                    _pleaseWaitService.Show("Удаление объекта...");
                    //имитация длительного процесса вычисления или получения данных
                    Thread.Sleep(2000);
                    BooksCollection.Remove(SelectedBook);

                    _pleaseWaitService.Hide();
                    currCollection.Clear();
                    foreach (var item in BooksCollection)
                    {
                        currCollection.Add(item);
                    }
                },
                () => SelectedBook != null));
            }
        }

        public string TextToSearch
        {
            get { return GetValue<string>(SearchProperty); }
            set { SetValue(SearchProperty, value); }
        }
        public static readonly PropertyData SearchProperty = RegisterProperty("TextToSearch", typeof(string));

        private Command _searchCommand;
        public Command SearchCommand
        {
            get
            {
                return _searchCommand ?? (_searchCommand = new Command(() =>
                {
                    ObservableCollection<Book> tempBookCollection = new ObservableCollection<Book>();
                    foreach (var item in BooksCollection)
                    {
                        if (item.Title.Contains(TextToSearch) || item.Author.Contains(TextToSearch))
                        {  
                            tempBookCollection.Add(item);
                        }
                    }
                    BooksCollection.Clear();
                    BooksCollection = tempBookCollection;
                },
                () => !string.IsNullOrWhiteSpace(TextToSearch)));
            }
        }

        private Command _loadDataCommand;
        public Command LoadDataCommand
        {
            get
            {
                return _loadDataCommand ?? (_loadDataCommand = new Command(() =>
                {
                    BooksCollection.Clear();
                    foreach (var item in currCollection)
                    {
                        BooksCollection.Add(item);
                    }
                    currCollection.Clear();
                },
                () => string.IsNullOrWhiteSpace(TextToSearch)));
            }
        }
    }
}
