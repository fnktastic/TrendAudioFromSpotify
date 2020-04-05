using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using TrendAudioFromSpotify.Data.DataAccess;
using TrendAudioFromSpotify.Data.Model;

namespace TrendAudioFromSpotify.Data.Repository
{
    public interface IAudioRepository
    {
        Task<List<AudioDto>> GetAllAsync();

        Task InsertAsync(AudioDto audio);
        Task InsertAudioRangeAsync(IEnumerable<AudioDto> audios);

        Task RemoveAsync(string audioId);
        Task RemoveAsync(AudioDto audio);
    }

    public class AudioRepository : IAudioRepository
    {
        private readonly Context _context;
        public AudioRepository(Context context)
        {
            _context = context;
        }

        public Task<List<AudioDto>> GetAllAsync()
        {
            return _context.Audios.ToListAsync();
        }

        public async Task RemoveAsync(string audioId)
        {
            var dbEntry = await _context.Audios.FindAsync(audioId);

            if (dbEntry != null)
            {
                _context.Entry<AudioDto>(dbEntry).State = EntityState.Deleted;

                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveAsync(AudioDto audio)
        {
            await RemoveAsync(audio.Id);
        }

        public async Task InsertAsync(AudioDto audio)
        {
            if (string.IsNullOrEmpty(audio.Id) == false)
            {
                var dbEntry = await _context.Audios.FindAsync(audio.Id);

                if (dbEntry == null)
                {
                    audio.CreatedAt = DateTime.UtcNow;
                    audio.UpdatedAt = DateTime.UtcNow;
                    _context.Audios.Add(audio);
                }
                else
                {
                    dbEntry.Title = audio.Title;
                    dbEntry.Artist = audio.Artist;
                    dbEntry.Href = audio.Href;
                    dbEntry.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task InsertAudioRangeAsync(IEnumerable<AudioDto> audios)
        {
            foreach (var audio in audios)
                await InsertAsync(audio);
        }
    }
}
