﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsTerm_Universal.ViewModels
{
    public class ItemModel
    {
        public int id { get; set; }
        public String title { get; set; }
        public String body { get; set; }
        public String author { get; set; }
        public String favicon { get; set; }

        public ItemModel()
        {
        }

        public static ItemModel FromItem(NextcloudNewsInterface.NextcloudNewsItem backendItem)
        {
            var newItem = new ItemModel();

            newItem.id = backendItem.id;
            newItem.title = backendItem.title;
            newItem.author = backendItem.author;
            newItem.body = backendItem.body;

            return newItem;
        }
    }
}
