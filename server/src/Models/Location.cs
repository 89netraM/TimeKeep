using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618

namespace TimeKeep.Models;

[Index(nameof(Name))]
public class Location
{
	public Guid Id { get; set; }
	public string? Name { get; set; }
	public string Address { get; set; }
	public ICollection<Entry> Entries { get; set; }
}
