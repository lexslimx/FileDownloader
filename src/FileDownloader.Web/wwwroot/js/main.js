function showLoader() {
    $("body").append("<div class='loader'></div>");
}

function hideLoader() {
    $('.loader').fadeOut('fast', function () { $(this).remove(); });
}

function getFiles() {
    $.get("/api/fileDownloads", function (data) {
        loadTable(data);
    })
        .fail(function () {
            alert("Error in getting files");
        })
        .always(function () {
            hideLoader();
        });
}

function loadTable(data) {
    $("#files").html("");

    if (data && data.length > 0) {
        for (var i = 0; i < data.length; i++) {
            var fileDownload = data[i];

            var row = "<tr><td>" + fileDownload.id + "</td>";
            row += "<td><a href='" + fileDownload.url + "' target='_blank'>" + fileDownload.fileName + "</a></td>";
            row += "<td>" + fileDownload.status + "</td>";
            row += "<td class='actions'><a  data-width='640' data-height='360' class='link' href='javascript:;' onclick='openLink(" + fileDownload.id + ",\"" + fileDownload.url + "\",\"" + fileDownload.fileType + "\")'>View</a>";

            if (fileDownload.status === "Ready") {
                row += "<a class='link green action link" + fileDownload.id + "' href='javascript:;' onclick='approveFile(" + fileDownload.id + ")'>Accept</a>";
                row += "<a class='link red action link" + fileDownload.id + "' href='javascript:;' onclick='rejectFile(" + fileDownload.id + ")'>Reject</a></td></tr>";
            }
            else if (fileDownload.status === "Accepted") {
                row += "<a class='link red link" + fileDownload.id + "' href='javascript:;' onclick='rejectFile(" + fileDownload.id + ")'>Reject</a></td></tr>";
            }
            else if (fileDownload.status === "Rejected") {
                row += "<a class='link green link" + fileDownload.id + "' href='javascript:;' onclick='approveFile(" + fileDownload.id + ")'>Accept</a></td></tr>";
            }

            $("#files").append(row);
        }
    }
    else {
        $('.table').hide();
        $("#message").show();
    }
}

function approveFile(id) {
    updateFileStatus(id, "Accepted");
}

function rejectFile(id) {
    updateFileStatus(id, "Rejected");
}

function updateFileStatus(id, status) {
    showLoader();

    $.ajax({
        url: "/api/fileDownloads/" + id,
        type: "PATCH",
        data: JSON.stringify({ fileStatus: status }),
        contentType: 'application/json'
    })
        .done(function (data) {
            getFiles();
        })
        .fail(function () {
            alert("Error updating status");
        })
        .always(function () {
            hideLoader();
        });
}

function openLink(id, url, fileType) {
    if (fileType !== "Other") {
        $.fancybox.open({
            src: url,
            type: 'iframe',
            iframe: {
                css: {
                    width: '508px',
                    height: '319px'
                }
            }
        });
    }
    else {
        window.location = url;
    }

    $('.link' + id).show();
}