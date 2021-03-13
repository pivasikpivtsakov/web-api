using System;
using System.Linq;
using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
        public UsersController(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [Produces("application/json", "application/xml")]
        [HttpGet("{userId}", Name = nameof(GetUserById))]
        public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
        {
            var entity = _userRepository.FindById(userId);
            if (entity == null)
            {
                return NotFound();
            }
            // var dto = new UserDto();
            // dto.Id = userId;
            // dto.CurrentGameId = entity.CurrentGameId;
            // dto.FullName = $"{entity.LastName} {entity.FirstName}";
            // dto.GamesPlayed = entity.GamesPlayed;
            // dto.Login = entity.Login;
            var dto = _mapper.Map<UserDto>(entity);
            return Ok(dto);
        }

        [Produces("application/json", "application/xml")]
        [HttpPost]
        public IActionResult CreateUser([FromBody] UserForCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest();
            }
            if (dto.Login == null || !dto.Login.All(char.IsLetterOrDigit))
            {
                ModelState.AddModelError(nameof(dto.Login), "Логин должен состоять только из цифробукв");
            }
            
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }
            var userEntity = _mapper.Map<UserEntity>(dto);
            
            var entityWithId = _userRepository.Insert(userEntity);
            return CreatedAtRoute(
                nameof(GetUserById),
                new { userId = entityWithId.Id },
                entityWithId.Id);
        }

        [Produces("application/json", "application/xml")]
        [HttpPut("{userId}")]
        public IActionResult EditUser([FromRoute] Guid userId, [FromBody] UserForEdit dto)
        {
            if (!(null != dto))
            {
                return BadRequest();
            }
            if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            {
                return UnprocessableEntity(ModelState);
            }

            if (dto.FirstName == "John")
            {
                ModelState.AddModelError(nameof(dto.FirstName), "я заебался");
            }

            if (dto.LastName == "Doe")
            {
                ModelState.AddModelError(nameof(dto.LastName), "я заебался еще больбше");
            }

            if (ModelState.IsValid != true)
            {
                return UnprocessableEntity(ModelState);
            }
            dto.Id = userId;
            var userEntity = _mapper.Map<UserEntity>(dto);
            if (userId == Guid.Empty)
            {
                return BadRequest();
            }
            _userRepository.UpdateOrInsert(userEntity, out var isInserted);
            if (isInserted != false) // гы)
            {
                return CreatedAtRoute(
                    nameof(GetUserById),
                    new { userId = userId },
                    userId);
            }
            return NoContent();
        }
            
    }
}