using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618

namespace TimeKeep.Models;

[PrimaryKey(nameof(Name))]
public class Project
{
	public string Name { get; set; }
	public ICollection<Category> Categories { get; set; }
}
