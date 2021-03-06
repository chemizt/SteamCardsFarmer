﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using SteamAPI.Data;
using SteamAPI.Data.Types;

namespace SteamAPI {
    public class SteamShopAPI {
        private readonly SteamGamesContext context;
        private readonly String baseUrl = "https://store.steampowered.com/search/?sort_by=Price_ASC&category1=998&category2=29";

        public SteamShopAPI() => context = new SteamGamesContext();

        public void ReloadGamesDB(double maxPrice) {
            foreach (var entity in context.SSGames.ToList())                 
                context.SSGames.Remove(entity);            

            foreach (var game in SteamShopParse(maxPrice))
                context.SSGames.Add(game.Value);

            context.SaveChanges();
        }

        public List<SSGame> GetGames() {
            var result = context.SSGames.ToList();
            if (result.Count > 0)
                return result;
            else
                throw new Exception("Database is empty!");
        }

        private Dictionary<string, SSGame> SteamShopParse(double maxPrice) {
            var url = baseUrl;
            Dictionary<string, SSGame> games = new Dictionary<string, SSGame>();

            using (var client = new WebClient()) {
                double currPrice = 0;
                do {
                    var html = client.DownloadString(url);
                    var document = new HtmlDocument();
                    document.LoadHtml(html);

                    var gameNodes = document.DocumentNode.SelectNodes(@"//div[@id = 'search_results']/div[@id = 'search_result_container']/div[2]/a");
                    if (gameNodes == null)
                        continue;
                    foreach (var node in gameNodes) {
                        var title = node.SelectSingleNode(@"div[2]/div[1]/span[@class = 'title']").InnerText;
                        var price = node.SelectSingleNode(@"div[2]/div[4]/div[2]").InnerText;
                        var href = node.GetAttributeValue(@"href", "error");

                        if (!price.Contains('.'))
                            continue;

                        var arr = price.Split(new[] { "pСѓР±." }, StringSplitOptions.None);     //разбиение по подстроке "руб."
                        price = (arr.Count() > 2) ? arr[1] : arr[0];

                        var game = new SSGame() {
                            Title = title,
                            Price = double.Parse(price),
                            Link = href
                        };
                        currPrice = game.Price;

                        try {
                            games.Add(game.Title, game);
                        }
                        catch (ArgumentException) {
                            continue;
                        }
                    }

                    var nextPageButtons = document.DocumentNode.SelectNodes(@"//div[@class = 'search_pagination_right']/a[@class = 'pagebtn']");
                    foreach (var button in nextPageButtons)
                        if (button.InnerText == "&gt;")         //знак >
                            url = button.GetAttributeValue(@"href", "error");
                        else
                            url = "end";

                } while (currPrice <= maxPrice && url != "end");
            }           
            
            return games;
        }

    }
}
