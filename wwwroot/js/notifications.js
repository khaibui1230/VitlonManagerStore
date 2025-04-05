/**
 * Hệ thống xử lý âm thanh thông báo cho ứng dụng Quán Vịt Lộn
 */

// Hàm tạo âm thanh thông báo sử dụng Web Audio API
function playNotificationSound() {
    try {
        // Tạo audio context
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        const audioCtx = new AudioContext();
        
        // Tạo oscillator để phát âm thanh
        const oscillator = audioCtx.createOscillator();
        const gainNode = audioCtx.createGain();
        
        // Kết nối các node
        oscillator.connect(gainNode);
        gainNode.connect(audioCtx.destination);
        
        // Thiết lập tham số
        oscillator.type = 'sine'; // Dạng âm thanh (sine, square, sawtooth, triangle)
        oscillator.frequency.value = 880; // Tần số 880Hz (note A5)
        gainNode.gain.value = 0.5; // Âm lượng (0-1)
        
        // Thiết lập envelope cho âm thanh
        gainNode.gain.setValueAtTime(0, audioCtx.currentTime);
        gainNode.gain.linearRampToValueAtTime(0.5, audioCtx.currentTime + 0.1);
        gainNode.gain.linearRampToValueAtTime(0, audioCtx.currentTime + 0.5);
        
        // Bắt đầu phát âm thanh
        oscillator.start();
        
        // Dừng oscillator sau khi phát xong
        oscillator.stop(audioCtx.currentTime + 0.5);
        
        return true;
    } catch (error) {
        console.error("Không thể phát âm thanh thông báo:", error);
        return false;
    }
}

// Hàm phát âm thanh "ding dong" thông báo
function playDingDongSound() {
    try {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        const audioCtx = new AudioContext();
        
        // Tạo hai oscillator cho hai nốt nhạc
        const osc1 = audioCtx.createOscillator();
        const osc2 = audioCtx.createOscillator();
        const gainNode = audioCtx.createGain();
        
        // Kết nối
        osc1.connect(gainNode);
        osc2.connect(gainNode);
        gainNode.connect(audioCtx.destination);
        
        // Thiết lập
        osc1.type = 'sine';
        osc2.type = 'sine';
        osc1.frequency.value = 783.99; // G5
        osc2.frequency.value = 523.25; // C5
        
        // Điều chỉnh âm lượng
        gainNode.gain.value = 0.3;
        
        // Nốt đầu tiên (Ding)
        osc1.start(audioCtx.currentTime);
        osc1.stop(audioCtx.currentTime + 0.2);
        
        // Nốt thứ hai (Dong)
        osc2.start(audioCtx.currentTime + 0.3);
        osc2.stop(audioCtx.currentTime + 0.5);
        
        return true;
    } catch (error) {
        console.error("Không thể phát âm thanh thông báo:", error);
        return false;
    }
}

// Hàm phát âm thanh "melodic" thông báo - phát 3 nốt nhạc liên tiếp
function playMelodicSound() {
    try {
        const AudioContext = window.AudioContext || window.webkitAudioContext;
        const audioCtx = new AudioContext();
        
        // Danh sách các nốt nhạc sẽ phát (C5, E5, G5)
        const notes = [523.25, 659.25, 783.99];
        const duration = 0.15; // Thời gian mỗi nốt
        const spacing = 0.05; // Khoảng cách giữa các nốt
        
        // Tạo gain node để điều chỉnh âm lượng
        const gainNode = audioCtx.createGain();
        gainNode.connect(audioCtx.destination);
        gainNode.gain.value = 0.3;
        
        // Phát từng nốt
        notes.forEach((frequency, index) => {
            // Tạo oscillator mới cho mỗi nốt
            const osc = audioCtx.createOscillator();
            osc.type = 'sine';
            osc.frequency.value = frequency;
            osc.connect(gainNode);
            
            // Thời gian bắt đầu và kết thúc cho nốt hiện tại
            const startTime = audioCtx.currentTime + (duration + spacing) * index;
            const stopTime = startTime + duration;
            
            // Phát nốt
            osc.start(startTime);
            osc.stop(stopTime);
        });
        
        return true;
    } catch (error) {
        console.error("Không thể phát âm thanh thông báo:", error);
        return false;
    }
} 