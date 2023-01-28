using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618

namespace TimeKeep.Models;

[PrimaryKey(nameof(Name))]
public class Category
{
	public string Name { get; set; }
	public ICollection<Entry> Entries { get; set; }
	public ICollection<Project> Projects { get; set; }
}
