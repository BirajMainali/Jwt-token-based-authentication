using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Data;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // api/Todo
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TodoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TodoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Todo()
        {
            try
            {
                var todos = await _context.Todos.ToListAsync();
                return Ok(todos);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetItem(long id)
        {
            try
            {
                var item = await _context.Todos.FirstOrDefaultAsync(x => x.Id == id);
                if (item == null) return NotFound();
                return Ok(item);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Todo todo)
        {
            try
            {
                if (!ModelState.IsValid) return new JsonResult("Something went wrong") { StatusCode = 500 };
                await _context.Todos.AddAsync(todo);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetItem", new { todo.Id }, todo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Complete(long id)
        {
            try
            {
                var todo = await _context.Todos.FirstOrDefaultAsync(x => x.Id == id);
                if (todo == null) return NotFound();
                todo.Done = true;
                await _context.AddAsync(todo);
                await _context.SaveChangesAsync();
                return Ok(todo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, Todo todo)
        {
            try
            {
                if (!ModelState.IsValid) return new JsonResult("Somethings went wrong") { StatusCode = 500 };
                if (id != todo.Id) return BadRequest();
                var existTodo = await _context.Todos.FirstOrDefaultAsync(x => x.Id == id);
                if (existTodo == null) return NotFound();

                existTodo.Title = todo.Title;
                existTodo.Description = todo.Description;
                existTodo.Done = todo.Done;
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Remove(long id)
        {
            try
            {
                var todo = await _context.Todos.FirstOrDefaultAsync(x => x.Id == id);
                if (todo == null) return NotFound();
                _context.Todos.Remove(todo);
                await _context.SaveChangesAsync();
                return Ok(todo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}