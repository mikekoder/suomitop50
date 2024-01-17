import pandas as pd
import os
import math

def get_songs():
    songs = []
    df = pd.read_excel('top75.xlsx')
    df["year"] = df["year"].astype(int)
    df["ranking"] = df["ranking"].astype(int)
    df["artist"] = df["artist"].astype(str)
    df["title"] = df["title"].astype(str)
    df["video"] = df["video"].astype(str)
    df["start"] = df["start"].astype(float)
    df["audiolevel"] = df["audiolevel"].astype(float)

    records = df.to_dict('records')
    for record in records:
        song = {
            'year': record['year'], 
            'ranking': record['ranking'], 
            'artist': record['artist'], 
            'title': record['title'], 
            'file': record['video'],
            'start': record['start'],
            'audio_level': record['audiolevel']
        }
        if math.isnan(song["audio_level"]):
            song["audio_level"] = 100
        songs.append(song)

    return songs

def get_videos(year, start, end):
    songs = get_songs()
    
    videos = []
    for ranking in range(start, end + 1):
        song = next((item for item in songs if item['year'] == year and item['ranking'] == ranking), None)
        folder = f'{year}/{ranking}'
        file = ''
        start = 0
        status = 'OK'
        if os.path.exists(folder):
            file = f'{folder}/{song["file"]}'

            if len(song["file"]) == 0:
                status = 'Video not selected'
            elif not os.path.exists(file):
                status = 'Video file missing'   
            elif math.isnan(song["start"]):
                status = 'Start not selected'
            else:
                status = 'OK'
        else:
            status = 'Directory missing'
        
        videos.append({
            'status': status, 
            'ranking': ranking, 
            'artist': song['artist'], 
            'title': song['title'],
            'file': file, 
            'start': song['start'], 
            'audio_level': song['audio_level']
            })
    return videos