using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AskMe.Models.QuestionsView
{
    public class IndexQuestion
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ViewCount { get; set; }
        public int AnswerCount { get; set; }
        public int FavorCount { get; set; }
        public System.DateTime CreateTime { get; set; }
        public List<string> Tags { get; set; }
    }
    public class CreateViewModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Detail { get; set; }
        public List<string> Tags { get; set; }
    }
    public class EditViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Detail { get; set; }
        public List<string> Tags { get; set; }
    }
}