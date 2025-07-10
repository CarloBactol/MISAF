// ================================ Config & Routes =================================
const routes = {
    getIndex: '/FinalApprover/Index',
    getRequestMain: '/FinalApprover/GetRequestMain',
    getRequestDetails: '/FinalApprover/GetRequestDetails',
    postSaveFinalApprover: '/FinalApprover/SaveFinalApproverAsync',
    postSaveAll: '/FinalApprover/PostSaveAll',
};

// ================================ Initialize DOM =================================
$(document).ready(function () {
    initToastrFinalApprover();
    loadInitialFinalApproverTable();
    document.getElementById("remarksModal")?.removeAttribute("aria-hidden");
    document.getElementById("remarksModal")?.focus();
    document.getElementById("approverModal")?.removeAttribute("aria-hidden");
    document.getElementById("approverModal")?.focus();
    document.getElementById("imageModal")?.removeAttribute("aria-hidden");
    document.getElementById("imageModal")?.focus();

    // Bind save buttons for remarksModal
    $('#btnSaveRemarksAcknowledge').click(function () {
        SaveFinalApprover({
            Index: $('#remarksIndex').val(),
            MAF_No: $('#remarksMAFNo').val(),
            Status: "For Acknowledgement MIS"
        });
    });

    $('#btnSaveRemarksReject').click(function () {
        SaveFinalApprover({
            Index: $('#remarksIndex').val(),
            MAF_No: $('#remarksMAFNo').val(),
            Status: 'Rejected'
        });
    });

    $('#fApproveAll').click(function () {
        $('#btnApproveAll').attr('data-maf-no', $(this).data('maf-no')).show();
        $('#btnRejectAll').hide();
        openModalFinaApprover('#remarksModalAll', clearRemarksModal);
    });

    $('#fRejectAll').click(function () {
        $('#btnRejectAll').attr('data-maf-no', $(this).data('maf-no')).show();
        $('#btnApproveAll').hide();
        openModalFinaApprover('#remarksModalAll', clearRemarksModal);
    });


    $('#btnApproveAll').off('click').on('click', function () {
        var remarksAll = $('#remarksAll').val().trim();
        if (remarksAll === '') {
            toastr.info("Remarks is required", "Info");
            return;
        } 
        SaveAll({
            MAF_No: $(this).data('maf-no'),
            Status: 'For Acknowledgement MIS',
            Remarks: $('#remarksAll').val()
        });
    });

    $('#btnRejectAll').off('click').on('click', function () {
        var remarksAll = $('#remarksAll').val().trim();
        if (remarksAll === '') {
            toastr.info("Remarks is required", "Info");
            return;
        }
        SaveAll({
            MAF_No: $(this).data('maf-no'),
            Status: 'Rejected',
            Remarks: $('#remarksAll').val()
        });
    });


});

// Set up global AJAX pre-loader
function setupGlobalAjaxPreloader() {
    $(document).ajaxStart(function () {
        $("#loading").show();
    }).ajaxStop(function () {
        $("#loading").hide();
    });
}

function initToastrFinalApprover() {
    toastr.options = {
        'closeButton': true,
        'debug': false,
        'newestOnTop': false,
        'progressBar': false,
        'positionClass': 'toast-top-right',
        'preventDuplicates': false,
        'showDuration': '1000',
        'hideDuration': '1000',
        'timeOut': '5000',
        'extendedTimeOut': '1000',
        'showEasing': 'swing',
        'hideEasing': 'linear',
        'showMethod': 'fadeIn',
        'hideMethod': 'fadeOut',
        'toastClass': 'custom-toastr',
        'zIndex': 10000 // Ensure Toastr is above pre-loader
    };
}

function loadInitialFinalApproverTable() {
    $.get(routes.getRequestMain, function (res) {
        if (res.success) {
            loadRequestMainTable(res.main, res.users);
        } else {
            toastr.error(res.message || 'Failed to load endorser table.', 'Error');
        }
    }).fail(function (xhr, status, error) {
        toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
    });
}

function loadRequestMainTable(data, user) {
    const table = $('#misafTable');
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#misafTableBody').empty();
    $.each(data, function (i, item) {
        if (item.Requestor_Name !== user.Endorser) {
            const statusDate = item.Status_Remarks ? formatDate(item.Status_DateTime) : '';
            const buttons = `<button type="button" data-maf-no="${item.MAF_No}"  class="btn btn-info btn-sm me-1 btn-approver" data-bs-toggle="tooltip" data-bs-title="Approve"><i class="mdi mdi-account-check"></i></button>`;


            let rowClass = '';
            if (item.Status === "Approved" || item.Status === "Done") {
                rowClass = 'table-success';
            } else if (item.Status === "Disapproved" || item.Status === "Rejected") {
                rowClass = 'table-danger';
            } else if (item.Status === "On Going" || item.Status === "For Acknowledgement MIS") {
                rowClass = 'table-warning';
            } else if (item.Status === "For Acknowledgement MIS") {
                rowClass = 'table-primary';
            } else if (item.Status === "On Hold") {
                rowClass = 'table-info';
            }

            body.append(`<tr class="${rowClass}">
                <td>${item.MAF_No}</td>
                <td>${item.Requested_By || item.Requestor_Name}</td>
                <td>${item.Requestor_Name}</td>
                <td>${item.Status || ''}</td>
                <td>${statusDate}</td>
                <td>${item.Status_Updated_By || ''}</td>
                <td>${item.Status_Remarks || ''}</td>
                <td>${buttons}</td>
            </tr>`);
        }
    });

    table.DataTable({
        paging: true,
        searching: true,
        ordering: true,
        lengthChange: true,
        lengthMenu: [[10, 50, 100], [10, 50, 100]]
    });

    $('[data-bs-toggle="tooltip"]').tooltip();
    $('.btn-approver').click(function (e) {
        finalApprover($(this).data("maf-no"), e);
    });


}

function loadRequestDetailTable(data) {
    const table = $('#requestTableEndorser');
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#requestTableBodyEndorser').empty();
    $.each(data, function (i, item) {
        let buttons = '';
        if (item.Status === "Approved" || (item.Status  === "For Approval" && item.Endorsed_By == null)) {
            buttons += `<button type="button" class="btn btn-success btn-sm me-1 mb-1 btn-approve" data-record-id="${item.Record_ID}" data-maf-no="${item.MAF_No}" data-bs-toggle="tooltip" data-bs-title="Approve"><i class="mdi mdi-check"></i></button>`;
            buttons += `<button type="button" class="btn btn-danger btn-sm me-1 mb-1 btn-reject" data-record-id="${item.Record_ID}" data-maf-no="${item.MAF_No}" data-bs-toggle="tooltip" data-bs-title="Disapprove"><i class="mdi mdi-cancel"></i></button>`;
        }

        const statusDate = (item.Status_Remarks || item.Status !== "For Approval") ? formatDate(item.Status_DateTime) : '';

        let rowClass = '';
        if (item.Status === "Approved" || item.Status === "Done") {
            rowClass = 'table-success';
        } else if (item.Status === "Disapproved" || item.Status === "Rejected") {
            rowClass = 'table-danger';
        } else if (item.Status === "On Going" || item.Status === "For Acknowledgement MIS") {
            rowClass = 'table-warning';
        } else if (item.Status === "For Acknowledgement MIS") {
            rowClass = 'table-primary';
        } else if (item.Status === "On Hold") {
            rowClass = 'table-info';
        }

        body.append(`<tr class="${rowClass}">
            <td>${item.Category}</td>
            <td>${item.Request}</td>
            <td>${item.Status}</td>
            <td>${statusDate}</td>
            <td>${item.Status_Remarks || ''}</td>
            <td>${buttons}</td>
        </tr>`);
    });

    table.DataTable({
        bInfo: false,
        paging: false,
        searching: true,
        ordering: true,
        lengthChange: false,
        lengthMenu: [[50, 100], [50, 100]]
    });

    $('[data-bs-toggle="tooltip"]').tooltip();

    // Bind approve/disapprove buttons
    $('.btn-approve').off('click').on('click', function () {
        approveFinalApprover($(this).data('record-id'), $(this).data('maf-no'));
    });

    $('.btn-reject').off('click').on('click', function () {
        disapprove($(this).data('record-id'), $(this).data('maf-no'));
    });
}

function loadSelectedRequestMainMisaf(data) {
    const date1 = formatDotNetDate(data.DateTime_Endorsed);
    const date2 = formatDotNetDate(data.DateTime_Approved);
    const date3 = formatDotNetDate(data.Status_DateTime);
    $('#MAF_No').val(data.MAF_No);
    $('#Requested_By').val(data.Requested_By || data.Requestor_Name);
    $('#Requested_For').val(data.Requested_By != null ? data.Requestor_Name : null);
    $('#Endorsed_By').val(data.Endorsed_By || 'N/A');
    $('#DateTime_Endorsed').val(data.Endorsed_By == null ? 'N/A' : date1);
    $('#Endorser_Remarks').val(data.Endorser_Remarks || 'N/A');
    $('#Final_Approver').val(data.Final_Approver);
    $('#DateTime_Approved').val(date2);
    $('#Final_Approver_Remarks').val(data.Final_Approver_Remarks);
    $('#Status').val(data.Status);
    $('#Status_DateTime1').val(date3);
    $('#mafNoAcknowledge').val(data.MAF_No);
    $('#preApproved').prop('checked', data.PreApproved === 'Y');
    $('#btnSaveAcknowledge').toggle(data.Status !== 'On Going');
    $('#fApproveAll').attr('data-maf-no', data.MAF_No);
    $('#fRejectAll').attr('data-maf-no', data.MAF_No);
    $("#fApproveAll").prop('disabled', false);
    $("#fRejectAll").prop('disabled', false);

}

function loadAttachmentFinalApproverTable(data) {
    const table = $('#attachmentTableApprover');
    attachmentData = data;
    if ($.fn.DataTable.isDataTable(table)) table.DataTable().destroy();

    const body = $('#attachmentTableBodyApprover').empty();
    $.each(data, function (i, item) {
        const file = `${item.MAF_No}-${item.Record_ID}-${item.Filename}`;
        const ext = item.Filename.split('.').pop().toLowerCase();
        const isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(ext);
        const fileUrl = `/Attachment/GetAttachment?fileName=${encodeURIComponent(file)}`;

        const preview = isImage
            ? `<img src="${fileUrl}" style="max-width: 80px; max-height: 80px; cursor: pointer;" onclick="showImageModalAcknowledge('${fileUrl}')">`
            : `<a href="${fileUrl}" target="_blank">${item.Filename}</a>`;

        body.append(`
            <tr>
                <td>${item.Filename}</td>
                <td>${preview}</td>
                <td>
                    <button onclick="viewAttachmentAcknowledge(${i}, event)" class="btn btn-info btn-sm" data-bs-toggle="tooltip" data-bs-title="View"><i class="mdi mdi-eye"></i></button>
                </td>
            </tr>`);
    });

    table.DataTable({
        bInfo: false,
        paging: false,
        searching: true,
        ordering: true,
        lengthChange: false
    });

    $('[data-bs-toggle="tooltip"]').tooltip();
}

function showImageModalAcknowledge(fileUrl) {
    const modal = $('#imageModal');
    const modalImage = modal.find('#modalImage');
    modalImage.attr('src', fileUrl);
    modal.modal('show');
}

function viewAttachmentAcknowledge(index, event) {
    event.preventDefault();
    const item = attachmentData[index];
    const file = `${item.MAF_No}-${item.Record_ID}-${item.Filename}`;
    const ext = item.Filename.split('.').pop().toLowerCase();
    const isImage = ['jpg', 'jpeg', 'png', 'gif'].includes(ext);
    const fileUrl = `/Attachment/GetAttachment?fileName=${encodeURIComponent(file)}`;

    if (isImage) {
        showImageModalAcknowledge(fileUrl);
    } else {
        window.open(fileUrl, '_blank');
    }
}

// ================================ Endorser Management =================================
function finalApprover(mafNo, event) {
    if (event) event.preventDefault();
    $.get(routes.getRequestDetails, { mafNo }, function (res) {
        if (res.success) {
            $('#approveModalLabel').text('Final Approver Management');
            openModalFinaApprover('#approverModal', clearFinalApproverModal);
            loadRequestDetailTable(res.detail);
            loadSelectedRequestMainMisaf(res.main);

            // Collect Status values from res.details array
            let remarks = [];
            for (let item of res.detail) {
                if (item && item.Status_Remarks && item.Status_Updated_By == res.user.UserLogin) {
                    remarks.push(item.Status_Remarks);
                }
            }

            // Set Endorser_Remarks field with joined statuses
            $('#Final_Approver_Remarks').empty();
            $('#Final_Approver_Remarks').val(remarks.join(','));
            $("#fApproveAll").prop('disabled', false);
            $("#fRejectAll").prop('disabled', false);

            loadAttachmentFinalApproverTable(res.attachment);
        } else {
            toastr.info(res.message, 'Info');
        }
    }).fail(function (xhr, status, error) {
        toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
    });
}

function approveFinalApprover(recordID, mafNo, event) {
    if (event) event.preventDefault();
    $('#remarksIndex').val(recordID);
    $('#remarksMAFNo').val(mafNo);
    $('#remarksStatus').val('Approved');
    $('#remarks').val('');
    $('#btnSaveRemarksAcknowledge').show();
    $('#btnSaveRemarksReject').hide();
    $('#remarksModal').modal('show');
}

function disapprove(recordID, mafNo, event) {
    if (event) event.preventDefault();
    $('#remarksIndex').val(recordID);
    $('#remarksMAFNo').val(mafNo);
    $('#remarksStatus').val('Disapprove');
    $('#remarks').val('');
    $('#btnSaveRemarksAcknowledge').hide();
    $('#btnSaveRemarksReject').show();
    $('#remarksModal').modal('show');
}

function SaveFinalApprover(originalData) {
    if (originalData.Status === "For Acknowledgement MIS") {
        if (!confirm(`Are you sure you want to approve request ${originalData.MAF_No}?`)) return;
    } else if (originalData.Status === "Rejected") {
        if (!confirm(`Are you sure you want to disapprove request ${originalData.MAF_No}?`)) return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();
    const request = {
        Index: originalData.Index,
        MAF_No: originalData.MAF_No,
        Status: originalData.Status,
        Remarks: $('#remarks').val()
    };

    $("#loading").show();

    $.ajax({
        url: routes.postSaveFinalApprover,
        type: 'POST',
        data: { request, __RequestVerificationToken: token },
        timeout: 30000,
        success: function (res) {
            if (!res || !res.success) {
                toastr.error(res?.message || 'An unexpected error occurred.', 'Error');
                console.error('SaveFinalApprover failed:', res);

                return;
            }

            setTimeout(() => {
                loadRequestDetailTable(res.details);

                // Collect Status values from res.details array
                let remarks = [];
                for (let item of res.details) {
                    if (item && item.Status_Remarks && item.Status_Updated_By == res.users.UserLogin) {
                        remarks.push(item.Status_Remarks);
                    }
                }

                // Set Endorser_Remarks field with joined statuses
                $('#Final_Approver_Remarks').val(remarks.join(','));
                $('#remarksModal').modal('hide');
                toastr.success(res.message, 'Success', { timeOut: 5000 });
            }, 400);
        },
        error: function (xhr, status, error) {
            toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
            console.error('SaveFinalApprover AJAX error:', status, error);
        },
        complete: function (res) {
            $("#loading").hide();
            //loadRequestMainTable(res.main, res.users)
            if (res.done) {
                $("#fApproveAll").prop('disabled', true);
                $("#fRejectAll").prop('disabled', true);
            }
            if (!res.done) {
                $("#fApproveAll").prop('disabled', false);
                $("#fRejectAll").prop('disabled', false);
            }
            loadInitialFinalApproverTable();
        }
    });
}

function SaveAll(data) {
    if (data.Status === "For Acknowledgement MIS") {
        if (!confirm(`Are you sure you want to approve request ${data.MAF_No}?`)) return;
    } else if (data.Status === "Rejected") {
        if (!confirm(`Are you sure you want to reject request ${data.MAF_No}?`)) return;
    }

    const token = $('input[name="__RequestVerificationToken"]').val();
    const request = {
        MAF_No: data.MAF_No,
        Status: data.Status,
        Remarks: data.Remarks
    };

    $("#loading").show();

    $.ajax({
        url: routes.postSaveAll,
        type: 'POST',
        data: { request, __RequestVerificationToken: token },
        timeout: 30000,
        success: function (res) {
            if (!res || !res.success) {
                toastr.error(res?.message || 'An unexpected error occurred.', 'Error');
                console.error('SaveFinalApprover failed:', res);

                return;
            }
            setTimeout(() => {
                loadRequestDetailTable(res.details);
                // Collect Status values from res.details array
                let remarks = [];
                for (let item of res.details) {
                    if (item && item.Status_Remarks && item.Status_Updated_By == res.users.UserLogin) {
                        remarks.push(item.Status_Remarks);
                    }
                }

                // Set Endorser_Remarks field with joined statuses
                $('#Final_Approver_Remarks').val(remarks.join(','));
                $('#remarksModalAll').modal('hide');
                toastr.success(res.message, 'Success', { timeOut: 5000 });
            }, 400);
        },
        error: function (xhr, status, error) {
            toastr.error(`Request failed: ${error || 'Unable to connect to the server.'}`, 'Error');
            console.error('SaveApproveALl AJAX error:', status, error);
        },
        complete: function (res) {
            $("#loading").hide();
            $("#fApproveAll").prop('disabled', true);
            $("#fRejectAll").prop('disabled', true);
            //loadRequestMainTable(res.main, res.users)
            loadInitialFinalApproverTable();
        }
    });
}

// ================================ Modals =================================
function clearFinalApproverModal() {
    $('#requestTableBodyEndorser').empty();
    $('#attachmentTableBodyApprover').empty();
    $('#MAF_No, #Requested_By, #Requested_For, #Endorsed_By, #DateTime_Endorsed, #Endorser_Remarks, #Final_Approver, #DateTime_Approved, #Final_Approver_Remarks, #Status, #Status_DateTime1').val('');
    $('#preApproved, #sendEmail').prop('checked', false);
}

function clearRemarksModal() {
    $('#remarksAll').val('');
}

function openModalFinaApprover(id, clearFn) {
    clearFn?.();
    $(id).modal('show');
}

